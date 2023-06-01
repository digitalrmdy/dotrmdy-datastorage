using JetBrains.Annotations;

namespace dotRMDY.DataStorage.Abstractions.Models
{
	[PublicAPI]
	public interface IRepositoryBaseEntity
	{
		string Id { get; set; }
	}
}