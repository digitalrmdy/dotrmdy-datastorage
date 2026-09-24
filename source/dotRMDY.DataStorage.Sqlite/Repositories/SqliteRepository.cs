using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using dotRMDY.DataStorage.Abstractions.Models;
using dotRMDY.DataStorage.Abstractions.Repositories;
using dotRMDY.DataStorage.Sqlite.Databases;
using SQLite;

namespace dotRMDY.DataStorage.Sqlite.Repositories;

public class SqliteRepository<TDomain, TStored>(ISqliteDb database) : IRepository<TDomain>
	where TDomain : class, IRepositoryBaseEntity
	where TStored : StoredEntity<TDomain, TStored>, new()
{
	private readonly SemaphoreSlim _tableInitializationSemaphore = new(1, 1);
	private bool _tableInitialized;

	public async Task<int> Count()
	{
		var connection = await GetConnection();

		return await connection
			.Table<TStored>()
			.CountAsync();
	}

	public async Task<IEnumerable<TDomain>> GetAll()
	{
		var storedItems = await GetAllStoredItems();

		return storedItems
			.Select(x => x.ToDomain())
			.ToList();
	}

	public async Task<TDomain?> GetForId(string id)
	{
		var connection = await GetConnection();

		var storedItem = await connection
			.FindAsync<TStored>(id);

		return storedItem?.ToDomain();
	}

	public async Task UpsertItem(TDomain domainItem)
	{
		var connection = await GetConnection();

		await connection.InsertOrReplaceAsync(StoredEntity<TDomain, TStored>.FromDomain(domainItem));
	}

	public virtual async Task UpsertAllItems(IEnumerable<TDomain> domainItems, bool dropExistingRecords = false)
	{
		var storedItems = domainItems
			.Select(StoredEntity<TDomain, TStored>.FromDomain)
			.ToList();

		var connection = await GetConnection();

		await connection.RunInTransactionAsync(transaction =>
		{
			if (dropExistingRecords)
			{
				transaction.DeleteAll<TStored>();
			}

			foreach (var storedItem in storedItems)
			{
				transaction.InsertOrReplace(storedItem);
			}
		});
	}

	public async Task DeleteItem(string id)
	{
		var connection = await GetConnection();

		await connection.DeleteAsync<TStored>(id);
	}

	public async Task DropCollection()
	{
		var connection = await GetConnection();

		await connection.DeleteAllAsync<TStored>();
	}

	protected async Task<TDomain?> FindFirstOrDefaultByPredicate(Expression<Func<TStored, bool>> predicate)
	{
		var connection = await GetConnection();

		var storedItem = await connection
			.Table<TStored>()
			.Where(predicate)
			.FirstOrDefaultAsync();

		return storedItem?.ToDomain();
	}

	protected async Task<IList<TDomain>> FindAllByPredicate(Expression<Func<TStored, bool>> predicate)
	{
		var connection = await GetConnection();

		var storedItems = await connection
			.Table<TStored>()
			.Where(predicate)
			.ToListAsync();

		return storedItems
			.Select(x => x.ToDomain())
			.ToList();
	}

	protected async Task<IList<TStored>> FindAllStoredEntitiesByPredicate(Expression<Func<TStored, bool>> predicate)
	{
		var connection = await GetConnection();

		return await connection
			.Table<TStored>()
			.Where(predicate)
			.ToListAsync();
	}

	protected async Task DeleteAllByPredicate(Expression<Func<TStored, bool>> predicate)
	{
		var connection = await GetConnection();

		var itemsToDelete = await connection
			.Table<TStored>()
			.Where(predicate)
			.ToListAsync();

		await connection.RunInTransactionAsync(transaction =>
		{
			foreach (var item in itemsToDelete)
			{
				transaction.Delete(item);
			}
		});
	}

	protected async Task<List<TStored>> GetAllStoredItems()
	{
		var connection = await GetConnection();

		return await connection
			.Table<TStored>()
			.ToListAsync();
	}

	protected async Task<SQLiteAsyncConnection> GetConnection()
	{
		var connection = await database.GetConnection();

		await EnsureTableExists(connection);

		return connection;
	}

	private async Task EnsureTableExists(SQLiteAsyncConnection connection)
	{
		if (_tableInitialized) return;

		await _tableInitializationSemaphore.WaitAsync();

		try
		{
			if (_tableInitialized) return;

			await connection.CreateTableAsync<TStored>();
			_tableInitialized = true;
		}
		finally
		{
			_tableInitializationSemaphore.Release();
		}
	}

	protected static string GetTableName()
	{
		return typeof(TStored)
			.GetCustomAttributes(typeof(TableAttribute), true)
			.Cast<TableAttribute>()
			.Single()
			.Name;
	}
}