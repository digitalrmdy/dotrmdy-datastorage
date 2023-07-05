using System.IO;
using dotRMDY.DataStorage.LiteDB.Databases.Implementations;
using dotRMDY.DataStorage.LiteDB.Services;
using dotRMDY.TestingTools;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace dotRMDY.DataStorage.LiteDB.UnitTests.Databases.Implementations
{
	public class BaseDbTest : SutSupportingTest<TestBaseDb>
	{
		private ILogger<TestBaseDb> _logger = null!;
		private IDatabaseFolderPathProvider _databaseFolderPathProvider = null!;
		private IDatabaseKeyProvider _databaseKeyProvider = null!;

		protected override void SetupCustomSutDependencies(SutBuilder builder)
		{
			base.SetupCustomSutDependencies(builder);

			_logger = builder.AddFakedDependency<ILogger<TestBaseDb>>();
			_databaseFolderPathProvider = builder.AddFakedDependency<IDatabaseFolderPathProvider>();
			_databaseKeyProvider = builder.AddFakedDependency<IDatabaseKeyProvider>();
		}

		[Fact]
		public void GetDatabasePath()
		{
			// Arrange
			A.CallTo(() => _databaseFolderPathProvider.DatabaseFolderPath).Returns("DatabaseFolderPath");

			// Act
			var result = Sut.GetDatabasePath();

			// Assert
			result.Should().Be(Path.Combine("DatabaseFolderPath", "TestBaseDb.db"));
		}

		[Fact]
		public void GetDatabaseLogPath()
		{
			// Arrange
			A.CallTo(() => _databaseFolderPathProvider.DatabaseFolderPath).Returns("DatabaseFolderPath");

			// Act
			var result = Sut.GetDatabaseLogPath();

			// Assert
			result.Should().Be(Path.Combine("DatabaseFolderPath", "TestBaseDb-log.db"));
		}
	}

	public sealed class TestBaseDb : BaseDb
	{
		public TestBaseDb(ILogger<TestBaseDb> logger, IDatabaseFolderPathProvider databaseFolderPathProvider, IDatabaseKeyProvider databaseKeyProvider)
			: base(logger, databaseFolderPathProvider, databaseKeyProvider)
		{
		}

		protected override string DbName => "TestBaseDb.db";
	}
}