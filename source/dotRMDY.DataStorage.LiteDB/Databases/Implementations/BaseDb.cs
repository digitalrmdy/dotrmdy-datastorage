using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using dotRMDY.Components.Extensions;
using dotRMDY.DataStorage.LiteDB.Services;
using JetBrains.Annotations;
using LiteDB;
using LiteDB.Async;
using Microsoft.Extensions.Logging;

namespace dotRMDY.DataStorage.LiteDB.Databases.Implementations
{
	[PublicAPI]
	public abstract class BaseDb : IBaseDb
	{
		protected readonly ILogger<BaseDb> Logger;
		protected readonly IDatabaseFolderPathProvider DatabaseFolderPathProvider;
		protected readonly IDatabaseKeyProvider DatabaseKeyProvider;

		private readonly SemaphoreSlim _dbInitSemaphoreSlim = new(1, 1);

		private LiteDatabaseAsync? _databaseInstance;

		protected BaseDb(
			ILogger<BaseDb> logger,
			IDatabaseFolderPathProvider databaseFolderPathProvider,
			IDatabaseKeyProvider databaseKeyProvider)
		{
			Logger = logger;
			DatabaseFolderPathProvider = databaseFolderPathProvider;
			DatabaseKeyProvider = databaseKeyProvider;
		}

		protected virtual TimeSpan LockTimeout => TimeSpan.FromSeconds(90);

		protected abstract string DbName { get; }

		/// <remark>
		///    Based upon <a href="https://github.com/mbdavid/LiteDB/blob/84faf672758552f7dab7f806dfc769709e63455d/LiteDB/Utils/FileHelper.cs#L37-L40\">the available source code</a>.
		/// </remark>
		protected virtual string DbNameLog => Path.GetFileNameWithoutExtension(DbName) + "-log" + Path.GetExtension(DbName);

		public virtual BsonMapper? Mapper => default;

		public bool IsInitialized => _databaseInstance != null;

		public Task Initialize()
		{
			return InitializeInternal();
		}

		public async Task<ILiteDatabaseAsync> GetDatabaseInstance()
		{
			if (!IsInitialized)
			{
				await InitializeInternal();
			}

			return _databaseInstance!;
		}

		public string GetDatabasePath()
		{
			return Path.Combine(DatabaseFolderPathProvider.DatabaseFolderPath, DbName);
		}

		public string GetDatabaseLogPath()
		{
			return Path.Combine(DatabaseFolderPathProvider.DatabaseFolderPath, DbNameLog);
		}

		public async Task CheckPointAndDelete()
		{
			await _dbInitSemaphoreSlim.WaitAsync();
			await DbCheckPoint();
			_databaseInstance?.Dispose();
			File.Delete(GetDatabaseLogPath());
		}

		public async Task DropCollection(string collectionName)
		{
			var liteDatabaseAsync = await GetDatabaseInstance();
			try
			{
				await liteDatabaseAsync.DropCollectionAsync(collectionName);
				await liteDatabaseAsync.CheckpointAsync();
			}
			catch (Exception e)
			{
				Logger.LogError(e, "Failed to drop collection, retrying forced | Collection: {Collection}",
					collectionName);
			}
		}

		public void ReinitializeCollections()
		{
			_databaseInstance?.Dispose();
			_databaseInstance = null;

			ResetDatabase();
			InitializeInternal().GetAwaiter().GetResult();
		}

		public virtual void ResetDatabase()
		{
			var dbLogPath = GetDatabaseLogPath();
			if (File.Exists(dbLogPath))
			{
				File.Delete(dbLogPath);
			}

			var dbPath = GetDatabasePath();
			if (File.Exists(dbPath))
			{
				File.Delete(dbPath);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual async Task InitializeInternal(bool isRetry = false)
		{
			if (IsInitialized)
			{
				return;
			}

			await _dbInitSemaphoreSlim.WaitAsync();

			if (IsInitialized)
			{
				_dbInitSemaphoreSlim.Release();
				return;
			}

			Logger.LogDebug("Initializing {DatabaseType} ", GetType().GetRealTypeName());

			var dbPath = GetDatabasePath();
			Logger.LogDebug("Database path: {DbPath}", dbPath);

			try
			{
				var connectionString = await CreateDatabaseConnectionString(dbPath);

				_databaseInstance = new LiteDatabaseAsync(connectionString, Mapper) { Timeout = LockTimeout };

				await _databaseInstance!.CollectionExistsAsync("testdbkey");
			}
			catch (Exception exc)
			{
				Logger.LogWarning(exc, "Failed to init DB");

				if (isRetry)
				{
					throw;
				}

				_databaseInstance?.Dispose();
				_databaseInstance = null;
				ResetDatabase();

				_dbInitSemaphoreSlim.Release();

				await InitializeInternal(true);
				return;
			}

			_dbInitSemaphoreSlim.Release();
		}

		protected virtual async Task<ConnectionString> CreateDatabaseConnectionString(string dbPath)
		{
			return new ConnectionString
			{
				Filename = dbPath,
				Password = await DatabaseKeyProvider.GetDbKey()
			};
		}

		protected virtual async Task DbCheckPoint()
		{
			if (_databaseInstance == null)
			{
				return;
			}

			try
			{
				Logger.LogInformation("Db checkpoint Hitmarker");
				await _databaseInstance.CheckpointAsync().ConfigureAwait(false);
			}
			catch (Exception e)
			{
				Logger.LogError(e, "Error calling db checkpoint");
				File.Delete(GetDatabaseLogPath());
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_databaseInstance?.Dispose();
				_databaseInstance = null;
			}
		}

		~BaseDb()
		{
			Dispose(false);
		}
	}
}