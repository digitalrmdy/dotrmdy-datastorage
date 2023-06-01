using System;
using System.Threading.Tasks;
using dotRMDY.Components.Shared.Helpers;
using JetBrains.Annotations;
using LiteDB.Async;

namespace dotRMDY.DataStorage.LiteDB.Databases
{
	[PublicAPI]
	public interface IBaseDb : INeedAsyncInitialization, IDisposable
	{
		Task<ILiteDatabaseAsync> GetDatabaseInstance();

		string GetDatabasePath();
		string GetDatabaseLogPath();

		Task CheckPointAndDelete();

		Task DropCollection(string collectionName);

		void ReinitializeCollections();
		void ResetDatabase();
	}
}