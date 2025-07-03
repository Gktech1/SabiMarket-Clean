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

namespace SabiMarket.Infrastructure.Repositories;

public class SubscriptionRepository : GeneralRepository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(ApplicationDbContext context) : base(context) { }

    public void AddSubscription(Subscription product) => Create(product);

    public async Task<IEnumerable<Subscription>> GetAllSubscriptionForExport(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

    public async Task<Subscription> GetSubscriptionById(string id, bool trackChanges) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync();

    public async Task<PaginatorDto<IEnumerable<Subscription>>> GetPagedSubscription(PaginationFilter paginationFilter)
    {
        return await FindAll(false)
                   .Paginate(paginationFilter);
    }

    public async Task<PaginatorDto<IEnumerable<Subscription>>> SearchSubscription(string searchString, PaginationFilter paginationFilter)
    {
        return await FindAll(false).Include(x => x.Subscriber)
                       .Where(a => a.Subscriber.FirstName.Contains(searchString) ||
                       a.Subscriber.LastName.Contains(searchString))
                       .Paginate(paginationFilter);
    }

}
