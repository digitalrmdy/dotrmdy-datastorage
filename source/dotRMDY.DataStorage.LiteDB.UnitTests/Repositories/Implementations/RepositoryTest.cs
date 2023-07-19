using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using dotRMDY.DataStorage.LiteDB.Databases;
using dotRMDY.DataStorage.LiteDB.Repositories.Implementations;
using dotRMDY.DataStorage.LiteDB.UnitTests.TestHelpers;
using dotRMDY.TestingTools;
using FakeItEasy;
using FluentAssertions;
using LiteDB.Async;
using Microsoft.Extensions.Logging;
using Xunit;

namespace dotRMDY.DataStorage.LiteDB.UnitTests.Repositories.Implementations
{
	public class RepositoryTest : SutSupportingTest<TestRepository>
	{
		private IBaseDb _baseDb = null!;
		private ILiteDatabaseAsync _underlyingLiteDatabaseAsync = null!;
		private ILiteCollectionAsync<TestRepositoryEntity> _underlyingLiteCollectionAsync = null!;

		protected override void SetupCustomSutDependencies(SutBuilder builder)
		{
			base.SetupCustomSutDependencies(builder);

			(_baseDb, _underlyingLiteDatabaseAsync, _underlyingLiteCollectionAsync) = RepositorySetupHelpers.SetupDatabase<IBaseDb>(builder);
		}

		[Fact]
		public async Task GetAll()
		{
			// Arrange
			var items = new[] { A.Dummy<TestRepositoryEntity>() };
			A.CallTo(() => _underlyingLiteCollectionAsync.FindAllAsync()).Returns(items);

			// Act
			var result = await Sut.GetAll();

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 2 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappenedTwiceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.FindAllAsync()).MustHaveHappenedOnceExactly();
			result.Should().BeEquivalentTo(items);
		}

		[Fact]
		public async Task GetAll_NoItems()
		{
			// Arrange
			var items = Array.Empty<TestRepositoryEntity>();
			A.CallTo(() => _underlyingLiteCollectionAsync.FindAllAsync()).Returns(items);

			// Act
			var result = await Sut.GetAll();

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 2 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappenedTwiceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.FindAllAsync()).MustHaveHappenedOnceExactly();
			result.Should().BeEquivalentTo(items);
		}

		[Fact]
		public async Task GetAll_AlreadyInitialized()
		{
			// Arrange
			var items = new[] { A.Dummy<TestRepositoryEntity>() };
			A.CallTo(() => _underlyingLiteCollectionAsync.FindAllAsync()).Returns(items);

			_ = await Sut.GetAll();

			// Act
			var result = await Sut.GetAll();

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 3 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappened(3, Times.Exactly);
			A.CallTo(() => _underlyingLiteCollectionAsync.FindAllAsync()).MustHaveHappenedTwiceExactly();
			result.Should().BeEquivalentTo(items);
		}

		[Fact]
		public async Task GetForId()
		{
			// Arrange
			var id = A.Dummy<string>();
			var item = A.Dummy<TestRepositoryEntity>();
			A.CallTo(() => _underlyingLiteCollectionAsync.FindByIdAsync(id)).Returns(item);

			// Act
			var result = await Sut.GetForId(id);

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 2 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappenedTwiceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.FindByIdAsync(id)).MustHaveHappenedOnceExactly();
			result.Should().BeEquivalentTo(item);
		}

		[Fact]
		public async Task GetForId_NoItem()
		{
			// Arrange
			A.CallTo<Task<TestRepositoryEntity?>>(() => _underlyingLiteCollectionAsync.FindByIdAsync("id")!)
				.Returns((TestRepositoryEntity?) null);

			// Act
			var result = await Sut.GetForId("id");

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 2 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappenedTwiceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.FindByIdAsync("id")).MustHaveHappenedOnceExactly();
			result.Should().BeNull();
		}

		[Fact]
		public async Task GetForId_AlreadyInitialized()
		{
			// Arrange
			var id = A.Dummy<string>();
			var item = A.Dummy<TestRepositoryEntity>();
			A.CallTo(() => _underlyingLiteCollectionAsync.FindByIdAsync(id)).Returns(item);

			_ = await Sut.GetForId(id);

			// Act
			var result = await Sut.GetForId(id);

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 3 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappened(3, Times.Exactly);
			A.CallTo(() => _underlyingLiteCollectionAsync.FindByIdAsync(id)).MustHaveHappenedTwiceExactly();
			result.Should().BeEquivalentTo(item);
		}

		[Fact]
		public async Task FindItem()
		{
			// Arrange
			var item = A.Dummy<TestRepositoryEntity>();
			A.CallTo(() => _underlyingLiteCollectionAsync.FindOneAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._))
				.Returns(item);

			// Act
			var result = await Sut.FindItem(x => x.Id == "id");

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 2 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappenedTwiceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.FindOneAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._)).MustHaveHappenedOnceExactly();
			result.Should().BeEquivalentTo(item);
		}

		[Fact]
		public async Task FindItem_NoItem()
		{
			// Arrange
			A.CallTo<Task<TestRepositoryEntity?>>(() => _underlyingLiteCollectionAsync.FindOneAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._)!)
				.Returns((TestRepositoryEntity?) null);

			// Act
			var result = await Sut.FindItem(x => x.Id == "id");

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 2 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappenedTwiceExactly();
			A.CallTo<Task<TestRepositoryEntity?>>(() => _underlyingLiteCollectionAsync.FindOneAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._)!)
				.MustHaveHappenedOnceExactly();
			result.Should().BeNull();
		}

		[Fact]
		public async Task FindItem_AlreadyInitialized()
		{
			// Arrange
			var item = A.Dummy<TestRepositoryEntity>();
			A.CallTo(() => _underlyingLiteCollectionAsync.FindOneAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._))
				.Returns(item);

			_ = await Sut.FindItem(x => x.Id == "id");

			// Act
			var result = await Sut.FindItem(x => x.Id == "id");

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 3 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappened(3, Times.Exactly);
			A.CallTo(() => _underlyingLiteCollectionAsync.FindOneAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._)).MustHaveHappenedTwiceExactly();
			result.Should().BeEquivalentTo(item);
		}

		[Fact]
		public async Task QueryItems()
		{
			// Arrange
			var items = new[] { A.Dummy<TestRepositoryEntity>() };
			A.CallTo(() => _underlyingLiteCollectionAsync.FindAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._, An<int>._, An<int>._))
				.Returns(items);

			// Act
			var result = await Sut.QueryItems(x => x.Id == "id");

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 2 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappenedTwiceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.FindAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._, An<int>._, An<int>._))
				.MustHaveHappenedOnceExactly();
			result.Should().BeEquivalentTo(items);
		}

		[Fact]
		public async Task QueryItems_NoItems()
		{
			// Arrange
			A.CallTo(() => _underlyingLiteCollectionAsync.FindAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._, An<int>._, An<int>._))
				.Returns(Array.Empty<TestRepositoryEntity>());

			// Act
			var result = await Sut.QueryItems(x => x.Id == "id");

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 2 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappenedTwiceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.FindAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._, An<int>._, An<int>._))
				.MustHaveHappenedOnceExactly();
			result.Should().BeEmpty();
		}

		[Fact]
		public async Task QueryItems_AlreadyInitialized()
		{
			// Arrange
			var items = new[] { A.Dummy<TestRepositoryEntity>() };
			A.CallTo(() => _underlyingLiteCollectionAsync.FindAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._, An<int>._, An<int>._))
				.Returns(items);

			_ = await Sut.QueryItems(x => x.Id == "id");

			// Act
			var result = await Sut.QueryItems(x => x.Id == "id");

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 3 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappened(3, Times.Exactly);
			A.CallTo(() => _underlyingLiteCollectionAsync.FindAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._, An<int>._, An<int>._))
				.MustHaveHappenedTwiceExactly();
			result.Should().BeEquivalentTo(items);
		}

		[Fact]
		public async Task UpsertItem()
		{
			// Arrange
			var item = new TestRepositoryEntity();

			// Act
			await Sut.UpsertItem(item);

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 2 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappenedTwiceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.UpsertAsync(item)).MustHaveHappenedOnceExactly();
		}

		[Fact]
		public async Task UpsertItem_NullItem()
		{
			// Arrange
			TestRepositoryEntity? item = null;

			// Act
			await Sut.UpsertItem(item);

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustNotHaveHappened();
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustNotHaveHappened();
			A.CallTo(() => _underlyingLiteCollectionAsync.UpsertAsync(item)).MustNotHaveHappened();
		}

		[Fact]
		public async Task UpsertItem_AlreadyInitialized()
		{
			// Arrange
			var item = new TestRepositoryEntity();

			await Sut.UpsertItem(item);

			// Act
			await Sut.UpsertItem(item);

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 3 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappened(3, Times.Exactly);
			A.CallTo(() => _underlyingLiteCollectionAsync.UpsertAsync(item)).MustHaveHappenedTwiceExactly();
		}

		[Fact]
		public async Task UpsertAllItems_WithoutDropExistingRecords()
		{
			// Arrange
			var items = new[] { A.Dummy<TestRepositoryEntity>() };

			// Act
			await Sut.UpsertAllItems(items);

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 2 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappenedTwiceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.UpsertAsync(items)).MustHaveHappenedOnceExactly();
		}

		[Fact]
		public async Task UpsertAllItems_WithoutDropExistingRecords_AlreadyInitialized()
		{
			// Arrange
			var items = new[] { A.Dummy<TestRepositoryEntity>() };

			await Sut.UpsertAllItems(items);

			// Act
			await Sut.UpsertAllItems(items);

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 3 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappened(3, Times.Exactly);
			A.CallTo(() => _underlyingLiteCollectionAsync.UpsertAsync(items)).MustHaveHappenedTwiceExactly();
		}

		[Fact]
		public async Task UpsertAllItems_WithDropExistingRecords()
		{
			// Arrange
			var items = new[] { A.Dummy<TestRepositoryEntity>() };

			// Act
			await Sut.UpsertAllItems(items, true);

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 2 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappenedTwiceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.DeleteAllAsync()).MustHaveHappenedOnceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.InsertAsync(items)).MustHaveHappenedOnceExactly();
		}

		[Fact]
		public async Task UpsertAllItems_WithDropExistingRecords_AlreadyInitialized()
		{
			// Arrange
			var items = new[] { A.Dummy<TestRepositoryEntity>() };

			await Sut.UpsertAllItems(items, true);

			// Act
			await Sut.UpsertAllItems(items, true);

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 3 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappened(3, Times.Exactly);
			A.CallTo(() => _underlyingLiteCollectionAsync.DeleteAllAsync()).MustHaveHappenedTwiceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.InsertAsync(items)).MustHaveHappenedTwiceExactly();
		}

		[Fact]
		public async Task UpsertAllItems_NullItems()
		{
			// Arrange
			TestRepositoryEntity[]? items = null;

			// Act
			await Sut.UpsertAllItems(items);

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustNotHaveHappened();
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustNotHaveHappened();
			A.CallTo(() => _underlyingLiteCollectionAsync.UpsertAsync(items)).MustNotHaveHappened();
		}

		[Fact]
		public async Task DeleteItem()
		{
			// Act
			await Sut.DeleteItem("id");

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 2 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappenedTwiceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.DeleteAsync("id")).MustHaveHappenedOnceExactly();
		}

		[Fact]
		public async Task DeleteItem_AlreadyInitialized()
		{
			// Arrange
			await Sut.DeleteItem("id");

			// Act
			await Sut.DeleteItem("id");

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 3 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappened(3, Times.Exactly);
			A.CallTo(() => _underlyingLiteCollectionAsync.DeleteAsync("id")).MustHaveHappenedTwiceExactly();
		}

		[Fact]
		public async Task DeleteMany()
		{
			// Act
			await Sut.DeleteMany(x => x.Id == "id");

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 2 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappenedTwiceExactly();
			A.CallTo(() => _underlyingLiteCollectionAsync.DeleteManyAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._)!)
				.MustHaveHappenedOnceExactly();
		}

		[Fact]
		public async Task DeleteMany_AlreadyInitialized()
		{
			// Arrange
			await Sut.DeleteMany(x => x.Id == "id");

			// Act
			await Sut.DeleteMany(x => x.Id == "id");

			// Assert
			A.CallTo(() => _baseDb.Initialize()).MustHaveHappenedOnceExactly();
			// 3 times due to initial access for setting up indexes when initializing repository
			A.CallTo(() => _underlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustHaveHappened(3, Times.Exactly);
			A.CallTo(() => _underlyingLiteCollectionAsync.DeleteManyAsync(A<Expression<Func<TestRepositoryEntity, bool>>>._)!)
				.MustHaveHappenedTwiceExactly();
		}

		[Fact]
		public async Task DropCollection()
		{
			// Act
			await Sut.DropCollection();

			// Assert
			A.CallTo(() => _baseDb.DropCollection(A<string>._)).MustHaveHappenedOnceExactly();
		}
	}

	public sealed class TestRepository : Repository<TestRepositoryEntity>
	{
		public TestRepository(ILogger log, IBaseDb inboxDb) : base(log, inboxDb)
		{
		}
	}
}