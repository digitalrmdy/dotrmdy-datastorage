using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dotRMDY.Components.Extensions;
using dotRMDY.DataStorage.Abstractions.Models;
using dotRMDY.DataStorage.LiteDB.Databases;
using JetBrains.Annotations;
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

		public async Task Initialize()
		{
			if (_isInitialized)
			{
				return;
			}

			try
			{
				await _initSemaphoreSlim.WaitAsync();

				if (_isInitialized)
				{
					return;
				}

				Logger.LogDebug("Initializing {Type}", GetType().GetRealTypeName());
				await Task.WhenAll(DatabaseList.Select(db => db.Initialize())).ConfigureAwait(false);

				await EnsureIndexesInternal();

				_isInitialized = true;
			}
			finally
			{
				_initSemaphoreSlim.Release();
			}
		}

		protected virtual async Task<ILiteCollectionAsync<T>> GetCollection(IBaseDb db)
		{
			var databaseInstance = await db.GetDatabaseInstance();
			return databaseInstance.GetCollection<T>(CollectionName);
		}

		private async Task EnsureIndexesInternal()
		{
			foreach (var database in DatabaseList)
			{
				await EnsureIndexes(database);
			}
		}

		protected async Task EnsureIndexes(IBaseDb db)
		{
			var collection = await GetCollection(db);
			await EnsureIndexes(collection);
		}

		protected virtual Task EnsureIndexes(ILiteCollectionAsync<T> collection)
		{
			return Task.CompletedTask;
		}
	}
}