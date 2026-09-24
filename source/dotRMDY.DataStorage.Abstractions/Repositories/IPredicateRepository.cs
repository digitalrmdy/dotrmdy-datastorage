using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using dotRMDY.DataStorage.Abstractions.Models;
using JetBrains.Annotations;

namespace dotRMDY.DataStorage.Abstractions.Repositories;

[PublicAPI]
public interface IPredicateRepository<T> : IRepository<T>
	where T : class, IRepositoryBaseEntity
{
	Task<T?> FindItem(Expression<Func<T, bool>> predicate);
	Task<List<T>> QueryItems(Expression<Func<T, bool>> predicate);
	Task DeleteMany(Expression<Func<T, bool>> predicate);
}