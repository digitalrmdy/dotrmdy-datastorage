using System.Threading.Tasks;
using SQLite;

namespace dotRMDY.DataStorage.Sqlite.Databases;

public interface ISqliteDb
{
	string GetDatabasePath();
	Task<SQLiteAsyncConnection> GetConnection();
}