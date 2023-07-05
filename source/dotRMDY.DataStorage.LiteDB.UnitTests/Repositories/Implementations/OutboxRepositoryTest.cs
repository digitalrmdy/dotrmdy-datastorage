using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using dotRMDY.Components.Shared.Extensions;
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
	public class OutboxRepositoryTest : SutSupportingTest<TestOutboxRepository>
	{
		private IInboxDb _inboxDb = null!;
		private ILiteDatabaseAsync _inboxDbUnderlyingLiteDatabaseAsync = null!;
		private ILiteCollectionAsync<TestRepositoryEntity> _inboxDbUnderlyingLiteCollectionAsync = null!;

		private IOutboxDb _outboxDb = null!;
		private ILiteDatabaseAsync _outboxDbUnderlyingLiteDatabaseAsync = null!;
		private ILiteCollectionAsync<TestRepositoryEntity> _outboxDbUnderlyingLiteCollectionAsync = null!;

		protected override void SetupCustomSutDependencies(SutBuilder builder)
		{
			base.SetupCustomSutDependencies(builder);

			(_inboxDb, _inboxDbUnderlyingLiteDatabaseAsync, _inboxDbUnderlyingLiteCollectionAsync) = RepositorySetupHelpers.SetupDatabase<IInboxDb>(builder);
			(_outboxDb, _outboxDbUnderlyingLiteDatabaseAsync, _outboxDbUnderlyingLiteCollectionAsync) =
				RepositorySetupHelpers.SetupDatabase<IOutboxDb>(builder);
		}

		[Fact]
		public async Task GetAll()
		{
			// Arrange
			var outboxItems = new[]
			{
				new TestRepositoryEntity
				{
					Id = "id1",
					Data = "outbox"
				},
				new TestRepositoryEntity
				{
					Id = "id2",
					Data = "outbox"
				}
			};
			A.CallTo(() => _outboxDbUnderlyingLiteCollectionAsync.FindAllAsync())
				.Returns(outboxItems);

			var inboxItems = new[]
			{
				new TestRepositoryEntity
				{
					Id = "id2",
					Data = "inbox"
				},
				new TestRepositoryEntity
				{
					Id = "id3",
					Data = "inbox"
				}
			};
			A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.FindAllAsync())
				.Returns(inboxItems);

			// Act
			var results = await Sut.GetAll().ToListAsync();

			// Assert
			A.CallTo(() => _outboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _outboxDbUnderlyingLiteCollectionAsync.FindAllAsync())
					.MustHaveHappenedOnceExactly());

			A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.FindAllAsync())
					.MustHaveHappenedOnceExactly());

			results.Should().HaveCount(3);
			results.Where(x => x.Data == "outbox").Should().HaveCount(2);
			results.Where(x => x.Data == "inbox").Should().HaveCount(1);
		}

		[Fact]
		public async Task GetForId_WithoutOutboxItem()
		{
			// Arrange
			A.CallTo<Task<TestRepositoryEntity?>>(() => _outboxDbUnderlyingLiteCollectionAsync.FindByIdAsync("id")!)
				.Returns((TestRepositoryEntity?) null);

			var inboxEntity = new TestRepositoryEntity
			{
				Id = "id",
				Data = "inbox"
			};
			A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.FindByIdAsync("id"))
				.Returns(inboxEntity);

			// Act
			var result = await Sut.GetForId("id");

			// Assert
			A.CallTo(() => _outboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _outboxDbUnderlyingLiteCollectionAsync.FindByIdAsync("id"))
					.MustHaveHappenedOnceExactly());

			A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.FindByIdAsync("id"))
					.MustHaveHappenedOnceExactly());

			result.Should().NotBeNull();
			result.Should().BeSameAs(inboxEntity);
		}

		[Fact]
		public async Task GetForId_WithOutboxItem()
		{
			// Arrange
			var outboxEntity = new TestRepositoryEntity
			{
				Id = "id",
				Data = "outbox"
			};
			A.CallTo<Task<TestRepositoryEntity?>>(() => _outboxDbUnderlyingLiteCollectionAsync.FindByIdAsync("id")!)
				.Returns(outboxEntity);

			// Act
			var result = await Sut.GetForId("id");

			// Assert
			A.CallTo(() => _outboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _outboxDbUnderlyingLiteCollectionAsync.FindByIdAsync("id"))
					.MustHaveHappenedOnceExactly());

			A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustNotHaveHappened();
			A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.FindByIdAsync("id")).MustNotHaveHappened();

			result.Should().NotBeNull();
			result.Should().BeSameAs(outboxEntity);
		}


		[Fact]
		public async Task FindItem_WithOutboxItem()
		{
			// Arrange
			Expression<Func<TestRepositoryEntity, bool>> predicate = entity => entity.Id == "id";
			var outboxEntity = new TestRepositoryEntity
			{
				Id = "id",
				Data = "outbox"
			};
			A.CallTo<Task<TestRepositoryEntity?>>(() => _outboxDbUnderlyingLiteCollectionAsync.FindOneAsync(predicate)!)
				.Returns(outboxEntity);

			// Act
			var result = await Sut.FindItem(predicate);

			// Assert
			A.CallTo(() => _outboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _outboxDbUnderlyingLiteCollectionAsync.FindOneAsync(predicate))
					.MustHaveHappenedOnceExactly());

			A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustNotHaveHappened();
			A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.FindOneAsync(predicate)).MustNotHaveHappened();

			result.Should().NotBeNull();
			result.Should().BeSameAs(outboxEntity);
		}

		[Fact]
		public async Task FindItem_WithoutOutboxItem()
		{
			// Arrange
			Expression<Func<TestRepositoryEntity, bool>> predicate = entity => entity.Id == "id";
			A.CallTo<Task<TestRepositoryEntity?>>(() => _outboxDbUnderlyingLiteCollectionAsync.FindOneAsync(predicate)!)
				.Returns((TestRepositoryEntity?) null);

			var inboxEntity = new TestRepositoryEntity
			{
				Id = "id",
				Data = "inbox"
			};
			A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.FindOneAsync(predicate))
				.Returns(inboxEntity);

			// Act
			var result = await Sut.FindItem(predicate);

			// Assert
			A.CallTo(() => _outboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _outboxDbUnderlyingLiteCollectionAsync.FindOneAsync(predicate))
					.MustHaveHappenedOnceExactly());

			A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.FindOneAsync(predicate))
					.MustHaveHappenedOnceExactly());

			result.Should().NotBeNull();
			result.Should().BeSameAs(inboxEntity);
		}

		[Fact]
		public async Task QueryItems()
		{
			// Arrange
			Expression<Func<TestRepositoryEntity, bool>> predicate = entity => entity.Id.Contains("id");
			var outboxItems = new[]
			{
				new TestRepositoryEntity
				{
					Id = "id1",
					Data = "outbox"
				},
				new TestRepositoryEntity
				{
					Id = "id2",
					Data = "outbox"
				}
			};
			A.CallTo(() => _outboxDbUnderlyingLiteCollectionAsync.FindAsync(predicate, An<int>._, An<int>._))
				.Returns(outboxItems);

			var inboxItems = new[]
			{
				new TestRepositoryEntity
				{
					Id = "id2",
					Data = "inbox"
				},
				new TestRepositoryEntity
				{
					Id = "id3",
					Data = "inbox"
				}
			};
			A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.FindAsync(predicate, An<int>._, An<int>._))
				.Returns(inboxItems);

			// Act
			var results = await Sut.QueryItems(predicate);

			// Assert
			A.CallTo(() => _outboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _outboxDbUnderlyingLiteCollectionAsync.FindAsync(predicate, An<int>._, An<int>._))
					.MustHaveHappenedOnceExactly());

			A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.FindAsync(predicate, An<int>._, An<int>._))
					.MustHaveHappenedOnceExactly());

			results.Should().HaveCount(3);
			results.Where(x => x.Data == "outbox").Should().HaveCount(2);
			results.Where(x => x.Data == "inbox").Should().HaveCount(1);
		}

		[Fact]
		public async Task UpsertItem()
		{
			// Arrange
			TestRepositoryEntity? capturedEntity = null;
			A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.UpsertAsync(A<TestRepositoryEntity>._))
				.Invokes(call => capturedEntity = call.Arguments.Get<TestRepositoryEntity>(0));

			var entity = A.Dummy<TestRepositoryEntity>();

			// Act
			await Sut.UpsertItem(entity);

			// Assert
			A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.UpsertAsync(A<TestRepositoryEntity>._))
					.MustHaveHappenedOnceExactly());

			capturedEntity.Should().BeSameAs(entity);
		}

		[Fact]
		public async Task UpsertItem_NullItem()
		{
			// Arrange
			TestRepositoryEntity? entity = null;

			// Act
			await Sut.UpsertItem(entity);

			// Assert
			A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustNotHaveHappened();
			A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.UpsertAsync(A<TestRepositoryEntity>._)).MustNotHaveHappened();
		}

		[Fact]
		public async Task UpsertItems_WithoutDropExistingRecords()
		{
			// Arrange
			var dummyEntities = A.CollectionOfDummy<TestRepositoryEntity>(3);

			// Act
			await Sut.UpsertAllItems(dummyEntities, false);

			// Assert
			A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.UpsertAsync(dummyEntities))
					.MustHaveHappenedOnceExactly());
		}

		[Fact]
		public async Task UpsertAllItems_WithDropExistingRecords()
		{
			// Arrange
			var dummyEntities = A.CollectionOfDummy<TestRepositoryEntity>(3);

			// Act
			await Sut.UpsertAllItems(dummyEntities, true);

			// Assert
			A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.BeginTransactionAsync())
					.MustHaveHappenedOnceExactly())
				.Then(A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.DeleteAllAsync())
					.MustHaveHappenedOnceExactly())
				.Then(A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.InsertAsync(dummyEntities))
					.MustHaveHappenedOnceExactly())
				.Then(A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.CommitAsync())
					.MustHaveHappenedOnceExactly());
		}

		[Fact]
		public async Task UpsertAllItems_NullItems()
		{
			// Act
			await Sut.UpsertAllItems(null, false);

			// Assert
			A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._)).MustNotHaveHappened();
		}

		[Fact]
		public async Task UpdateItem()
		{
			// Arrange
			var outboxEntity = A.Dummy<TestRepositoryEntity>();

			// Act
			await Sut.UpdateItem(outboxEntity);

			// Assert
			A.CallTo(() => _outboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _outboxDbUnderlyingLiteCollectionAsync.UpsertAsync(outboxEntity))
					.MustHaveHappenedOnceExactly());
		}

		[Fact]
		public async Task DeleteItem()
		{
			// Act
			await Sut.DeleteItem("id");

			// Assert
			A.CallTo(() => _outboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _outboxDbUnderlyingLiteCollectionAsync.DeleteAsync("id"))
					.MustHaveHappenedOnceExactly());

			A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.DeleteAsync("id"))
					.MustHaveHappenedOnceExactly());
		}

		[Fact]
		public async Task DeleteMany()
		{
			// Arrange
			Expression<Func<TestRepositoryEntity, bool>> predicate = x => x.Id == "id";

			// Act
			await Sut.DeleteMany(predicate);

			// Assert
			A.CallTo(() => _outboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _outboxDbUnderlyingLiteCollectionAsync.DeleteManyAsync(predicate))
					.MustHaveHappenedOnceExactly());

			A.CallTo(() => _inboxDbUnderlyingLiteDatabaseAsync.GetCollection<TestRepositoryEntity>(A<string>._))
				.MustHaveHappenedOnceExactly()
				.Then(A.CallTo(() => _inboxDbUnderlyingLiteCollectionAsync.DeleteManyAsync(predicate))
					.MustHaveHappenedOnceExactly());
		}

		[Fact]
		public async Task DropCollection()
		{
			// Act
			await Sut.DropCollection();

			// Assert
			A.CallTo(() => _inboxDb.DropCollection(nameof(TestRepositoryEntity)))
				.MustHaveHappenedOnceExactly();
		}

		[Fact]
		public async Task DropOutboxCollection()
		{
			// Act
			await Sut.DropOutboxCollection();

			// Assert
			A.CallTo(() => _outboxDb.DropCollection(nameof(TestRepositoryEntity)))
				.MustHaveHappenedOnceExactly();
		}


		[Fact]
		public async Task Initialize()
		{
			// Act
			await Sut.Initialize();

			// Assert
			A.CallTo(() => _inboxDb.Initialize()).MustHaveHappenedOnceExactly();
			A.CallTo(() => _outboxDb.Initialize()).MustHaveHappenedOnceExactly();
		}

		[Fact]
		public async Task Initialize_AlreadyInitialized()
		{
			// Arrange
			await Sut.Initialize();

			// Act
			await Sut.Initialize();

			// Assert
			A.CallTo(() => _inboxDb.Initialize()).MustHaveHappenedOnceExactly();
			A.CallTo(() => _outboxDb.Initialize()).MustHaveHappenedOnceExactly();
		}
	}

	public sealed class TestOutboxRepository : OutboxRepository<TestRepositoryEntity>
	{
		public TestOutboxRepository(ILogger log, IInboxDb inboxDb, IOutboxDb outboxDb) : base(log, inboxDb, outboxDb)
		{
		}
	}

	public interface IInboxDb : IBaseDb
	{
	}

	public interface IOutboxDb : IBaseDb
	{
	}
}