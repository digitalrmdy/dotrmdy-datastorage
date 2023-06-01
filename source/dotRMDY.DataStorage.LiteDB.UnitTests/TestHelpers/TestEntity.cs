using dotRMDY.DataStorage.Abstractions.Models;

namespace dotRMDY.DataStorage.LiteDB.UnitTests.TestHelpers
{
	public class TestRepositoryEntity : IRepositoryBaseEntity
	{
		public string Id { get; set; } = null!;

		public string? Data { get; init; }
	}
}