using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;

namespace SabiMarket.Infrastructure.Utilities
{
    public static class Pagination
    {
        public static async Task<PaginatorDto<IEnumerable<TSource>>> Paginate<TSource>(this IQueryable<TSource> queryable,
            PaginationFilter paginationFilter)
            where TSource : class
        {
            var count = await queryable.CountAsync();

            paginationFilter ??= new PaginationFilter();

            var pageResult = new PaginatorDto<IEnumerable<TSource>>
            {
                PageSize = paginationFilter.PageSize,
                CurrentPage = paginationFilter.PageNumber,
                TotalItems = count
            };

            pageResult.NumberOfPages = count % pageResult.PageSize != 0
                ? count / pageResult.PageSize + 1
                : count / pageResult.PageSize;

            pageResult.PageItems = await queryable.Skip((pageResult.CurrentPage - 1) * pageResult.PageSize)
            .Take(pageResult.PageSize).ToListAsync();

            return pageResult;
        }
    }
}
