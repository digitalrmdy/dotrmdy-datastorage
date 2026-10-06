using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace dotRMDY.DataStorage.Sqlite.Repositories;

internal sealed class PolymorphicTypes
{
	internal const string TypeDiscriminatorPropertyName = "$type";

	private readonly Dictionary<Type, HashSet<Type>> _derivedTypes = new();

	public static PolymorphicTypes FromObjectGraph(
		object root,
		Type declaredRootType,
		JsonSerializerOptions serializerOptions)
	{
		var result = new PolymorphicTypes();
		var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
		result.AddObjectGraph(root, declaredRootType, serializerOptions, visited);

		return result;
	}

	public static PolymorphicTypes FromJson(string json)
	{
		var result = new PolymorphicTypes();
		var root = JsonNode.Parse(json);

		if (root != null)
		{
			result.AddJsonNode(root);
		}

		return result;
	}

	public bool TryGetDerivedTypes(Type baseType, out IReadOnlyCollection<Type> derivedTypes)
	{
		if (_derivedTypes.TryGetValue(baseType, out var types))
		{
			derivedTypes = types;
			return true;
		}

		derivedTypes = Array.Empty<Type>();
		return false;
	}

	private void AddObjectGraph(
		object? value,
		Type declaredType,
		JsonSerializerOptions serializerOptions,
		HashSet<object> visited)
	{
		if (value == null) return;

		var runtimeType = value.GetType();
		Add(declaredType, runtimeType);

		if (IsLeaf(runtimeType) || !runtimeType.IsValueType && !visited.Add(value)) return;

		if (value is IDictionary dictionary)
		{
			var valueType = GetDictionaryValueType(declaredType) ?? typeof(object);

			foreach (DictionaryEntry entry in dictionary)
			{
				AddObjectGraph(entry.Value, valueType, serializerOptions, visited);
			}

			return;
		}

		if (value is IEnumerable enumerable && value is not string)
		{
			var elementType = GetEnumerableElementType(declaredType) ?? typeof(object);

			foreach (var item in enumerable)
			{
				AddObjectGraph(item, elementType, serializerOptions, visited);
			}

			return;
		}

		var typeInfo = serializerOptions.GetTypeInfo(runtimeType);

		if (typeInfo.Kind != JsonTypeInfoKind.Object) return;

		foreach (var property in typeInfo.Properties.Where(property => property.Get != null))
		{
			AddObjectGraph(property.Get!(value), property.PropertyType, serializerOptions, visited);
		}
	}

	private void AddJsonNode(JsonNode node)
	{
		if (node is JsonObject jsonObject)
		{
			if (jsonObject[TypeDiscriminatorPropertyName]?.GetValue<string>() is { } typeName)
			{
				var derivedType = Type.GetType(typeName, throwOnError: true)!;
				AddToBaseTypes(derivedType);
			}

			foreach (var property in jsonObject)
			{
				if (property.Value != null)
				{
					AddJsonNode(property.Value);
				}
			}
		}
		else if (node is JsonArray jsonArray)
		{
			foreach (var item in jsonArray)
			{
				if (item != null)
				{
					AddJsonNode(item);
				}
			}
		}
	}

	private void Add(Type declaredType, Type runtimeType)
	{
		declaredType = Nullable.GetUnderlyingType(declaredType) ?? declaredType;

		if (declaredType == runtimeType || !declaredType.IsAssignableFrom(runtimeType)) return;
		if (!declaredType.IsClass && !declaredType.IsInterface) return;
		if (!runtimeType.IsClass || IsLeaf(runtimeType)) return;

		if (!_derivedTypes.TryGetValue(declaredType, out var types))
		{
			types = new HashSet<Type>();
			_derivedTypes.Add(declaredType, types);
		}

		types.Add(runtimeType);
	}

	private void AddToBaseTypes(Type derivedType)
	{
		for (var baseType = derivedType.BaseType; baseType != null; baseType = baseType.BaseType)
		{
			Add(baseType, derivedType);
		}

		foreach (var interfaceType in derivedType.GetInterfaces())
		{
			Add(interfaceType, derivedType);
		}
	}

	private static bool IsLeaf(Type type)
	{
		return type.IsPrimitive
			|| type.IsEnum
			|| type == typeof(string)
			|| type == typeof(decimal)
			|| type == typeof(DateTime)
			|| type == typeof(DateTimeOffset)
			|| type == typeof(Guid)
			|| type == typeof(TimeSpan)
			|| type == typeof(Uri);
	}

	private static Type? GetEnumerableElementType(Type type)
	{
		return GetGenericInterface(type, typeof(IEnumerable<>))?.GetGenericArguments()[0];
	}

	private static Type? GetDictionaryValueType(Type type)
	{
		return GetGenericInterface(type, typeof(IDictionary<,>))?.GetGenericArguments()[1]
			?? GetGenericInterface(type, typeof(IReadOnlyDictionary<,>))?.GetGenericArguments()[1];
	}

	private static Type? GetGenericInterface(Type type, Type genericTypeDefinition)
	{
		if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition) return type;

		return type.GetInterfaces()
			.FirstOrDefault(candidate => candidate.IsGenericType
				&& candidate.GetGenericTypeDefinition() == genericTypeDefinition);
	}
}
