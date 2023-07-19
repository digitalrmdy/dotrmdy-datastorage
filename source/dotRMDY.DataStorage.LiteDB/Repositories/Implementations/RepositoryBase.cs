using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dotRMDY.Components.Extensions;
using dotRMDY.DataStorage.Abstractions.Models;
using dotRMDY.DataStorage.LiteDB.Databases;
using JetBrains.Annotations;
using LiteDB;
using LiteDB.Async;
using Microsoft.Extensions.Logging;

namespace dotRMDY.DataStorage.LiteDB.Repositories.Implementations
{
	[PublicAPI]
	public abstract class RepositoryBase<T>
		where T : class, IRepositoryBaseEntity
	{
		private readonly SemaphoreSlim _initSemaphoreSlim;

		private bool _isInitialized;

		protected ILogger Logger { get; }
		protected abstract IEnumerable<IBaseDb> DatabaseList { get; }
		protected virtual string? CollectionName => null;

		protected RepositoryBase(ILogger logger)
		{
			Logger = logger;

			_initSemaphoreSlim = new SemaphoreSlim(1, 1);
		}

		protected virtual async Task<ILiteCollectionAsync<T>> GetCollection(IBaseDb db)
		{
			await Initialize().ConfigureAwait(false);

			return await GetCollectionInternal(db).ConfigureAwait(false);
		}

		protected async Task EnsureIndexes(IBaseDb db)
		{
			var collection = await GetCollectionInternal(db);
			await EnsureIndexes(collection);
		}

		protected virtual Task EnsureIndexes(ILiteCollectionAsync<T> collection)
		{
			return Task.CompletedTask;
		}

		protected virtual void ConfigureMapper(BsonMapper databaseMapper)
		{
		}

		private async Task Initialize()
		{
			if (_isInitialized && DatabaseList.All(db => db.IsInitialized))
			{
				return;
			}

			try
			{
				await _initSemaphoreSlim.WaitAsync();

				if (_isInitialized && DatabaseList.All(db => db.IsInitialized))
				{
					return;
				}

				await Task.WhenAll(DatabaseList.Select(InitializeInternal)).ConfigureAwait(false);

				_isInitialized = true;
			}
			finally
			{
				_initSemaphoreSlim.Release();
			}
		}

		private async Task InitializeInternal(IBaseDb db)
		{
			if (!db.IsInitialized)
			{
				Logger.LogDebug("Initializing {DatabaseType} for {RepositoryType}",
					db.GetType().GetRealTypeName(),
					GetType().GetRealTypeName());
				await db.Initialize().ConfigureAwait(false);

				await EnsureIndexes(db).ConfigureAwait(false);
				ConfigureMapper(db.Mapper ?? BsonMapper.Global);
			}
		}

		private async Task<ILiteCollectionAsync<T>> GetCollectionInternal(IBaseDb db)
		{
			var databaseInstance = await db.GetDatabaseInstance();
			return databaseInstance.GetCollection<T>(CollectionName);
		}
	}
}