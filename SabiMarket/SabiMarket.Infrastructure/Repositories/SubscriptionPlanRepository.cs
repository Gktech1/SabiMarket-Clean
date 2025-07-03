using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class SubscriptionPlanRepository : GeneralRepository<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        public SubscriptionPlanRepository(ApplicationDbContext context) : base(context) { }

        public void AddSubscriptionPlan(SubscriptionPlan plan) => Create(plan);
        public void UpdateSubscriptionPlan(SubscriptionPlan plan) => Update(plan);

        public async Task<IEnumerable<SubscriptionPlan>> GetAllSubscriptionPlanForExport(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

        public async Task<SubscriptionPlan> GetSubscriptionPlanById(string id, bool trackChanges) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync();

        public async Task<PaginatorDto<IEnumerable<SubscriptionPlan>>> GetPagedSubscriptionPlan(PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                       .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<SubscriptionPlan>>> SearchSubscriptionPlan(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a =>
                           a.Frequency.Contains(searchString))
                           .Paginate(paginationFilter);
        }

    }
}
