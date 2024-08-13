using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using dotRMDY.DataStorage.Abstractions.Models;
using JetBrains.Annotations;

namespace dotRMDY.DataStorage.Abstractions.Repositories
{
	[PublicAPI]
	public interface IRepository<T>
		where T : class, IRepositoryBaseEntity
	{
		Task<int> Count();
		Task<IEnumerable<T>> GetAll();
		Task<T?> GetForId(string id);
		Task<T?> FindItem(Expression<Func<T, bool>> predicate);
		Task<List<T>> QueryItems(Expression<Func<T, bool>> predicate);

		Task UpsertItem(T item);
		Task UpsertAllItems(IEnumerable<T> model, bool dropExistingRecords = false);

		Task DeleteItem(string id);
		Task DeleteMany(Expression<Func<T, bool>> predicate);

		Task DropCollection();
	}
}