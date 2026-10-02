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

	public static TStored FromDomain(TDomain domain, bool usePolymorphicSerialization = false)
	{
		var stored = new TStored
		{
			Id = domain.Id,
			Json = usePolymorphicSerialization
				? StoredEntitySerializer.SerializePolymorphic(domain)
				: StoredEntitySerializer.Serialize(domain)
		};

		stored.MapCustomFields(domain);

		return stored;
	}

	public TDomain ToDomain(bool usePolymorphicSerialization = false)
	{
		return usePolymorphicSerialization
			? StoredEntitySerializer.DeserializePolymorphic<TDomain>(Json)!
			: StoredEntitySerializer.Deserialize<TDomain>(Json)!;
	}

	protected virtual void MapCustomFields(TDomain domain)
	{
	}
}
