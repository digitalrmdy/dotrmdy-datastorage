using System;
using System.Text.Json;

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

	public static string Serialize(object data)
	{
		return JsonSerializer.Serialize(data, data.GetType(), _serializerOptions);
	}

	public static T? Deserialize<T>(string data)
	{
		return JsonSerializer.Deserialize<T>(data, _serializerOptions);
	}

	public static object? Deserialize(string data, Type type)
	{
		return JsonSerializer.Deserialize(data, type, _serializerOptions);
	}
}
