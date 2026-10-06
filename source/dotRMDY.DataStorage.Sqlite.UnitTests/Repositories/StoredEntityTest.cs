using System.Collections.Generic;
using dotRMDY.DataStorage.Abstractions.Models;
using dotRMDY.DataStorage.Sqlite.Repositories;
using FluentAssertions;
using Xunit;

namespace dotRMDY.DataStorage.Sqlite.UnitTests.Repositories;

public class StoredEntityTest
{
	[Fact]
	public void FromDomainAndToDomain_WithNestedRuntimeTypes()
	{
		// Arrange
		var domain = new TestEntity
		{
			Id = "entity",
			ScalarValue = 42,
			PrimaryAnimal = new Dog { Name = "Pixel", FavoriteToy = "Ball" },
			Animals =
			[
				new Dog { Name = "Pixel", FavoriteToy = "Ball" },
				new Cat { Name = "Minoes", Lives = 9 }
			]
		};

		// Act
		var stored = TestStoredEntity.FromDomain(domain, usePolymorphicSerialization: true);
		var restored = stored.ToDomain(usePolymorphicSerialization: true);

		// Assert
		restored.PrimaryAnimal.Should().BeOfType<Dog>()
			.Which.FavoriteToy.Should().Be("Ball");
		restored.Animals.Should().HaveCount(2);
		restored.Animals[0].Should().BeOfType<Dog>();
		restored.Animals[1].Should().BeOfType<Cat>()
			.Which.Lives.Should().Be(9);
		stored.Json.Should().Contain("\"$type\"");
	}

	[Fact]
	public void FromDomainAndToDomain_WithRootRuntimeType()
	{
		// Arrange
		TestEntity domain = new DerivedTestEntity
		{
			Id = "derived",
			ExtraValue = "extra"
		};

		// Act
		var stored = TestStoredEntity.FromDomain(domain, usePolymorphicSerialization: true);
		var restored = stored.ToDomain(usePolymorphicSerialization: true);

		// Assert
		stored.Json.Should().StartWith("{\"$type\"");
		restored.Should().BeOfType<DerivedTestEntity>()
			.Which.ExtraValue.Should().Be("extra");
	}

	[Fact]
	public void FromDomainAndToDomain_WithoutPolymorphism()
	{
		// Arrange
		var domain = new TestEntity { Id = "regular" };

		// Act
		var stored = TestStoredEntity.FromDomain(domain);
		var restored = stored.ToDomain();
		var restoredThroughPolymorphicFastPath = stored.ToDomain(usePolymorphicSerialization: true);

		// Assert
		stored.Json.Should().NotContain("\"$type\"");
		restored.Id.Should().Be("regular");
		restoredThroughPolymorphicFastPath.Id.Should().Be("regular");
	}

	private sealed class TestStoredEntity : StoredEntity<TestEntity, TestStoredEntity>;

	private class TestEntity : IRepositoryBaseEntity
	{
		public string Id { get; set; } = null!;
		public object? ScalarValue { get; init; }
		public Animal? PrimaryAnimal { get; init; }
		public List<Animal> Animals { get; init; } = [];
	}

	private sealed class DerivedTestEntity : TestEntity
	{
		public string ExtraValue { get; init; } = null!;
	}

	private abstract class Animal
	{
		public string Name { get; init; } = null!;
	}

	private sealed class Dog : Animal
	{
		public string FavoriteToy { get; init; } = null!;
	}

	private sealed class Cat : Animal
	{
		public int Lives { get; init; }
	}
}
