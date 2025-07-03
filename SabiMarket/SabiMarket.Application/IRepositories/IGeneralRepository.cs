using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs;

namespace SabiMarket.Application.IRepositories
{
    public interface IGeneralRepository<T>
    {
        IQueryable<T> FindAll(bool trackChanges);
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression,
        bool trackChanges);
        void Create(T entity);
        void AddRannge(List<T> enities);
        void Update(T entity);
        void Delete(T entity);
        Task<PaginatorDto<IEnumerable<T>>> FindPagedByCondition(Expression<Func<T, bool>> expression, PaginationFilter paginationFilter, bool trackChanges, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
        Task<PaginatorDto<IEnumerable<T>>> FindPagedByCondition(PaginationFilter paginationFilter, bool trackChanges, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    }
}
