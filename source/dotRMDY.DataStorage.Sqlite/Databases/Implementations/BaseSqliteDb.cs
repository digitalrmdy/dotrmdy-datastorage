using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using dotRMDY.DataStorage.Sqlite.Services;
using SQLite;

namespace dotRMDY.DataStorage.Sqlite.Databases.Implementations;

public abstract class BaseSqliteDb(ISqliteDatabaseFolderPathProvider sqliteDatabaseFolderPathProvider) : ISqliteDb, IDisposable
{
	private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);

	private SQLiteAsyncConnection? _connection;

	protected abstract string DbName { get; }

	public async Task<SQLiteAsyncConnection> GetConnection()
	{
		if (_connection != null) return _connection;

		await _initializationSemaphore
			.WaitAsync()
			.ConfigureAwait(false);

		try
		{
			if (_connection != null) return _connection;

			_connection = new SQLiteAsyncConnection(GetDatabasePath());

			return _connection;
		}
		finally
		{
			_initializationSemaphore.Release();
		}
	}


	public string GetDatabasePath()
	{
		return Path.Combine(sqliteDatabaseFolderPathProvider.DatabaseFolderPath, DbName);
	}

	public void Dispose()
	{
		if (_connection != null)
		{
			_connection
				.CloseAsync()
				.GetAwaiter()
				.GetResult();

			_connection = null;
		}

		_initializationSemaphore.Dispose();
		GC.SuppressFinalize(this);
	}
}