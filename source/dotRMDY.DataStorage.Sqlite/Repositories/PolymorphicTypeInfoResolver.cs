using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace dotRMDY.DataStorage.Sqlite.Repositories;

internal sealed class PolymorphicTypeInfoResolver(IJsonTypeInfoResolver resolver,
	PolymorphicTypes polymorphicTypes) : IJsonTypeInfoResolver
{
	public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
	{
		var typeInfo = resolver.GetTypeInfo(type, options);

		if (typeInfo is not { Kind: JsonTypeInfoKind.Object } || !polymorphicTypes.TryGetDerivedTypes(type, out var derivedTypes))
			return typeInfo;

		if (typeInfo.PolymorphismOptions != null) return typeInfo;

		typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
		{
			TypeDiscriminatorPropertyName = PolymorphicTypes.TypeDiscriminatorPropertyName
		};

		foreach (var derivedType in derivedTypes)
		{
			typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType, GetTypeName(derivedType)));
		}

		return typeInfo;
	}

	private static string GetTypeName(Type type)
	{
		return $"{type.FullName}, {type.Assembly.GetName().Name}";
	}
}
