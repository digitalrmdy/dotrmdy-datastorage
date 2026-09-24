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

	public static TStored FromDomain(TDomain domain)
	{
		var stored = new TStored
		{
			Id = domain.Id,
			Json = StoredEntitySerializer.Serialize(domain)
		};

		stored.MapCustomFields(domain);

		return stored;
	}

	protected virtual void MapCustomFields(TDomain domain)
	{
	}

	public TDomain ToDomain()
	{
		return StoredEntitySerializer.Deserialize<TDomain>(Json)!;
	}
}