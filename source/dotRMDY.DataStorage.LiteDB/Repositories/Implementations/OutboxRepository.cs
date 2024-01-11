using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using dotRMDY.DataStorage.Abstractions.Models;
using dotRMDY.DataStorage.Abstractions.Repositories;
using dotRMDY.DataStorage.LiteDB.Databases;
using dotRMDY.DataStorage.LiteDB.Helpers;
using JetBrains.Annotations;
using LiteDB.Async;
using Microsoft.Extensions.Logging;

namespace dotRMDY.DataStorage.LiteDB.Repositories.Implementations
{
	[PublicAPI]
	public class OutboxRepository<T> : RepositoryBase<T>, IOutboxRepository<T>
		where T : class, IRepositoryBaseEntity
	{
		protected sealed override IEnumerable<IBaseDb> DatabaseList => new[] { InboxDb, OutboxDb };

		protected virtual IEqualityComparer<T> UnionComparer { get; } = new DefaultRepositoryBaseEntityEqualityComparer<T>();

		protected readonly IBaseDb InboxDb;
		protected readonly IBaseDb OutboxDb;

		protected OutboxRepository(
			ILogger logger,
			IBaseDb inboxDb,
			IBaseDb outboxDb)
			: base(logger)
		{
			InboxDb = inboxDb;
			OutboxDb = outboxDb;
		}

		public virtual async Task<IEnumerable<T>> GetAll()
		{
			var inboxCollection = await GetInboxCollection();
			var outboxCollection = await GetOutboxCollection();

			var outboxItems = await outboxCollection.FindAllAsync();
			var inboxItems = await inboxCollection.FindAllAsync();
			return outboxItems.Union(inboxItems, UnionComparer);
		}

		public virtual async Task<T?> GetForId(string id)
		{
			var outboxCollection = await GetOutboxCollection();
			var outboxEntity = await outboxCollection.FindByIdAsync(id);
			if (outboxEntity != null)
			{
				return outboxEntity;
			}

			var inboxCollection = await GetInboxCollection();
			return await inboxCollection.FindByIdAsync(id);
		}

		public virtual async Task<T?> FindItem(Expression<Func<T, bool>> predicate)
		{
			var outboxCollection = await GetOutboxCollection();
			var outboxEntity = await outboxCollection.FindOneAsync(predicate);
			if (outboxEntity != null)
			{
				return outboxEntity;
			}

			var inboxCollection = await GetInboxCollection();
			return await inboxCollection.FindOneAsync(predicate);
		}

		public virtual async Task<List<T>> QueryItems(Expression<Func<T, bool>> predicate)
		{
			var inboxCollection = await GetInboxCollection();
			var outboxCollection = await GetOutboxCollection();

			var outboxResult = await outboxCollection.FindAsync(predicate);
			var inboxResult = await inboxCollection.FindAsync(predicate);
			return outboxResult.Union(inboxResult, UnionComparer).ToList();
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

		public virtual async Task UpsertAllItems(IEnumerable<T> models, bool dropExistingRecords)
		{
			if (models == null)
			{
				return;
			}

			var inboxCollection = await GetInboxCollection();

			if (dropExistingRecords)
			{
				await inboxCollection.DeleteAllAsync();
				await inboxCollection.InsertAsync(models);
			}
			else
			{
				await inboxCollection.UpsertAsync(models);
			}
		}

		public virtual async Task UpdateItem(T model)
		{
			var outboxCollection = await GetOutboxCollection();
			await outboxCollection.UpsertAsync(model);
		}

		public virtual Task DeleteItem(string id)
		{
			return Task.WhenAll(DatabaseList.Select(async x =>
			{
				var collection = await GetCollection(x);
				await collection.DeleteAsync(id);
			}));
		}

		public virtual Task DeleteMany(Expression<Func<T, bool>> predicate)
		{
			return Task.WhenAll(DatabaseList.Select(async x =>
			{
				var collection = await GetCollection(x);
				await collection.DeleteManyAsync(predicate);
			}));
		}

		public virtual async Task DropCollection()
		{
			await InboxDb.DropCollection(CollectionName ?? typeof(T).Name);
			await EnsureIndexes(InboxDb);
		}

		public virtual async Task DropOutboxCollection()
		{
			await OutboxDb.DropCollection(CollectionName ?? typeof(T).Name);
			await EnsureIndexes(OutboxDb);
		}

		protected virtual Task<ILiteCollectionAsync<T>> GetInboxCollection()
		{
			return GetCollection(InboxDb);
		}

		protected virtual Task<ILiteCollectionAsync<T>> GetOutboxCollection()
		{
			return GetCollection(OutboxDb);
		}
	}
}