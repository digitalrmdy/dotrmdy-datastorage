using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using dotRMDY.Components.Extensions;
using dotRMDY.DataStorage.Abstractions.Models;
using dotRMDY.DataStorage.Abstractions.Repositories;
using dotRMDY.DataStorage.LiteDB.Databases;
using JetBrains.Annotations;
using LiteDB;
using LiteDB.Async;
using Microsoft.Extensions.Logging;

namespace dotRMDY.DataStorage.LiteDB.Repositories.Implementations
{
	[PublicAPI]
	public abstract class Repository<T> : RepositoryBase<T>, IRepository<T>
		where T : class, IRepositoryBaseEntity
	{
		protected override IEnumerable<IBaseDb> DatabaseList => new[] { InboxDb };

		protected IBaseDb InboxDb { get; }

		protected Repository(
			ILogger log,
			IBaseDb inboxDb)
			: base(log)
		{
			InboxDb = inboxDb;
		}

		public virtual async Task<IEnumerable<T>> GetAll()
		{
			var inboxCollection = await GetInboxCollection();
			return await inboxCollection.FindAllAsync();
		}

		public virtual async Task<T?> GetForId(string id)
		{
			var inboxCollection = await GetInboxCollection();
			return await inboxCollection.FindByIdAsync(id);
		}

		public virtual async Task<T?> FindItem(Expression<Func<T, bool>> predicate)
		{
			var inboxCollection = await GetInboxCollection();
			return await inboxCollection.FindOneAsync(predicate);
		}

		public virtual async Task<List<T>> QueryItems(Expression<Func<T, bool>> predicate)
		{
			var inboxCollection = await GetInboxCollection();
			return await inboxCollection.FindAsync(predicate).ToListAsync();
		}

		public virtual async Task UpsertItem(T item)
		{
			if (item == null)
			{
				return;
			}

			var inboxCollection = await GetInboxCollection();
			await inboxCollection.UpsertAsync(item);
		}

		public virtual async Task UpsertAllItems(IEnumerable<T> items, bool dropExistingRecords = false)
		{
			if (items == null)
			{
				return;
			}

			var inboxCollection = await GetInboxCollection();
			if (dropExistingRecords)
			{
				await inboxCollection.DeleteAllAsync();
				await inboxCollection.InsertAsync(items);
			}
			else
			{
				await inboxCollection.UpsertAsync(items);
			}
		}

		public virtual async Task DeleteItem(string id)
		{
			var inboxCollection = await GetInboxCollection();
			await inboxCollection.DeleteAsync(new BsonValue(id));
		}

		public virtual async Task DeleteMany(Expression<Func<T, bool>> predicate)
		{
			var inboxCollection = await GetInboxCollection();
			await inboxCollection.DeleteManyAsync(predicate);
		}

		public virtual async Task DropCollection()
		{
			await InboxDb.DropCollection(CollectionName ?? typeof(T).Name);
			await EnsureIndexes(InboxDb);
		}

		protected virtual Task<ILiteCollectionAsync<T>> GetInboxCollection()
		{
			return GetCollection(InboxDb);
		}
	}
}