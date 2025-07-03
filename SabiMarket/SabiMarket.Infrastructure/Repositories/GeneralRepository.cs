using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public abstract class GeneralRepository<T> : IGeneralRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;

        public GeneralRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Create(T entity) => _context.Set<T>().Add(entity);
        public void AddRannge(List<T> enities) => _context.Set<T>().AddRange(enities);
        public void Update(T entity) => _context.Set<T>().Update(entity);
        public void Delete(T entity) => _context.Set<T>().Remove(entity);

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges)
        {
            return !trackChanges ? _context.Set<T>().Where(expression).AsNoTracking() : _context.Set<T>().Where(expression);
        }

        public IQueryable<T> FindAll(bool trackChanges)
        {
            return !trackChanges ? _context.Set<T>().AsNoTracking() : _context.Set<T>();
        }
        public async Task<PaginatorDto<IEnumerable<T>>> FindPagedByCondition(Expression<Func<T, bool>> expression, PaginationFilter paginationFilter, bool trackChanges, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            var query = FindByCondition(expression, trackChanges);

            // Apply sorting if provided
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.Paginate(paginationFilter);
        }
        public async Task<PaginatorDto<IEnumerable<T>>> FindPagedByCondition(PaginationFilter paginationFilter, bool trackChanges, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            var query = FindAll(trackChanges);

            // Apply sorting if provided
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.Paginate(paginationFilter);
        }
    }

}
