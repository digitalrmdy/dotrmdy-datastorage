using dotRMDY.DataStorage.LiteDB.Databases;
using dotRMDY.TestingTools;
using FakeItEasy;
using LiteDB.Async;

namespace dotRMDY.DataStorage.LiteDB.UnitTests.TestHelpers
{
	public static class RepositorySetupHelpers
	{
		public static (TBaseDb, ILiteDatabaseAsync, ILiteCollectionAsync<TestRepositoryEntity>) SetupDatabase<TBaseDb>(
			SutBuilder builder)
			where TBaseDb : class, IBaseDb
		{
			var baseDb = builder.AddFakedDependency<TBaseDb>();
			var underlyingDatabaseAsync = A.Fake<ILiteDatabaseAsync>();
			var underlyingLiteCollectionAsync = A.Fake<ILiteCollectionAsync<TestRepositoryEntity>>();

			A.CallTo(() => underlyingDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.Returns(underlyingLiteCollectionAsync);
			A.CallTo(() => baseDb.GetDatabaseInstance())
				.Returns(underlyingDatabaseAsync);

			return (baseDb, underlyingDatabaseAsync, underlyingLiteCollectionAsync);
		}
	}
}