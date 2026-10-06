using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace dotRMDY.DataStorage.Sqlite.Repositories;

public static class StoredEntitySerializer
{
	private static JsonSerializerOptions _serializerOptions = JsonSerializerOptions.Default;

	// Configure once during app startup, before any repositories are used.
	public static void Configure(Action<JsonSerializerOptions> configure)
	{
		var options = new JsonSerializerOptions();

		configure(options);
		options.MakeReadOnly(populateMissingResolver: true);

		_serializerOptions = options;
	}

	public static string Serialize<T>(T data)
		where T : class
	{
		return JsonSerializer.Serialize(data, typeof(T), _serializerOptions);
	}

	public static string SerializePolymorphic<T>(T data)
		where T : class
	{
		var polymorphicTypes = PolymorphicTypes.FromObjectGraph(data, typeof(T), _serializerOptions);
		var options = CreateOptions(polymorphicTypes);

		return JsonSerializer.Serialize(data, typeof(T), options);
	}

	public static T? Deserialize<T>(string data)
	{
		return JsonSerializer.Deserialize<T>(data, _serializerOptions);
	}

	public static T? DeserializePolymorphic<T>(string data)
	{
		if (!data.Contains($"\"{PolymorphicTypes.TypeDiscriminatorPropertyName}\"", StringComparison.Ordinal)) return Deserialize<T>(data);

		var polymorphicTypes = PolymorphicTypes.FromJson(data);
		var options = CreateOptions(polymorphicTypes);

		return JsonSerializer.Deserialize<T>(data, options);
	}

	private static JsonSerializerOptions CreateOptions(PolymorphicTypes polymorphicTypes)
	{
		var options = new JsonSerializerOptions(_serializerOptions);
		var resolver = options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver();
		options.TypeInfoResolver = new PolymorphicTypeInfoResolver(resolver, polymorphicTypes);

		return options;
	}
}
