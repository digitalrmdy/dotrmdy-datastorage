using dotRMDY.DataStorage.Abstractions.Models;
using dotRMDY.DataStorage.LiteDB.Helpers;
using dotRMDY.DataStorage.LiteDB.UnitTests.TestHelpers;
using dotRMDY.TestingTools;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace dotRMDY.DataStorage.LiteDB.UnitTests.Helpers
{
	public class DefaultRepositoryBaseEntityEqualityComparerTest
		: SutSupportingTest<DefaultRepositoryBaseEntityEqualityComparer<IRepositoryBaseEntity>>
	{
		[Fact]
		public void Equals_ReferenceEquals()
		{
			// Arrange
			var entityA = A.Fake<IRepositoryBaseEntity>();
			var entityB = entityA;

			// Act
			var result = Sut.Equals(entityA, entityB);

			// Assert
			result.Should().BeTrue();
		}

		[Fact]
		public void Equals_ReferenceEquals_EntityANull()
		{
			// Arrange
			var entityB = A.Fake<IRepositoryBaseEntity>();

			// Act
			var result = Sut.Equals(null, entityB);

			// Assert
			result.Should().BeFalse();
		}

		[Fact]
		public void Equals_ReferenceEquals_EntityBNull()
		{
			// Arrange
			var entityA = A.Fake<IRepositoryBaseEntity>();

			// Act
			var result = Sut.Equals(entityA, null);

			// Assert
			result.Should().BeFalse();
		}

		[Fact]
		public void Equals_TypeEquality()
		{
			// Arrange
			var entityA = A.Fake<IRepositoryBaseEntity>();
			var entityB = A.Fake<TestRepositoryEntity>();

			// Act
			var result = Sut.Equals(entityA, entityB);

			// Assert
			result.Should().BeFalse();
		}

		[Fact]
		public void Equals_IdEquality()
		{
			// Arrange
			var entityA = A.Fake<IRepositoryBaseEntity>();
			A.CallTo(() => entityA.Id).Returns("id");

			var entityB = A.Fake<IRepositoryBaseEntity>();
			A.CallTo(() => entityB.Id).Returns("id");

			// Act
			var result = Sut.Equals(entityA, entityB);

			// Assert
			result.Should().BeTrue();
		}

		[Fact]
		public void HashCode()
		{
			// Arrange
			var entity = A.Fake<IRepositoryBaseEntity>();
			var expectedHashCode = entity.Id.GetHashCode();

			// Act
			var hashCode = Sut.GetHashCode(entity);

			// Assert
			hashCode.Should().Be(expectedHashCode);
		}
	}
}