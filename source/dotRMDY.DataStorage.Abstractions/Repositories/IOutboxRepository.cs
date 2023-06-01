using System.Threading.Tasks;
using dotRMDY.DataStorage.Abstractions.Models;
using JetBrains.Annotations;

namespace dotRMDY.DataStorage.Abstractions.Repositories
{
	[PublicAPI]
	public interface IOutboxRepository<T> : IRepository<T> where T : class, IRepositoryBaseEntity
	{
		Task UpdateItem(T model);

		Task DropOutboxCollection();
	}
}