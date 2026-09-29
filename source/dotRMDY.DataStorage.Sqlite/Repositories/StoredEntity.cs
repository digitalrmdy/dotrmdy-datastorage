using System;
using dotRMDY.DataStorage.Abstractions.Models;
using SQLite;

namespace dotRMDY.DataStorage.Sqlite.Repositories;

public abstract class StoredEntity<TDomain, TStored>
	where TDomain : class, IRepositoryBaseEntity
	where TStored : StoredEntity<TDomain, TStored>, new()
{
	[PrimaryKey]
	public string Id { get; init; } = null!;

	public string Json { get; init; } = null!;
	public string? PolymorphicTypeName { get; init; }

	public static TStored FromDomain(TDomain domain)
	{
		var runtimeType = domain.GetType();
		var stored = new TStored
		{
			Id = domain.Id,
			Json = StoredEntitySerializer.Serialize(domain),
			PolymorphicTypeName = runtimeType == typeof(TDomain) ? null : GetTypeName(runtimeType)
		};

		stored.MapCustomFields(domain);

		return stored;
	}

	public TDomain ToDomain()
	{
		if (PolymorphicTypeName == null)
		{
			return StoredEntitySerializer.Deserialize<TDomain>(Json)!;
		}

		var runtimeType = Type.GetType(PolymorphicTypeName, throwOnError: true)!;

		if (!typeof(TDomain).IsAssignableFrom(runtimeType))
		{
			throw new InvalidOperationException(
				$"Stored type '{runtimeType.FullName}' is not assignable to '{typeof(TDomain).FullName}'.");
		}

		return (TDomain)StoredEntitySerializer.Deserialize(Json, runtimeType)!;
	}

	protected virtual void MapCustomFields(TDomain domain)
	{
	}

	private static string GetTypeName(Type type)
	{
		return $"{type.FullName}, {type.Assembly.GetName().Name}";
	}
}
