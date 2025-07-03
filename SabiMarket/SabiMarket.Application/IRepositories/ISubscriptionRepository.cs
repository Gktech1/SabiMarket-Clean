using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Application.IRepositories
{
    public interface ISubscriptionRepository
    {
        void AddSubscription(Subscription product);
        Task<IEnumerable<Subscription>> GetAllSubscriptionForExport(bool trackChanges);
        Task<PaginatorDto<IEnumerable<Subscription>>> GetPagedSubscription(PaginationFilter paginationFilter);
        Task<Subscription> GetSubscriptionById(string id, bool trackChanges);
        Task<PaginatorDto<IEnumerable<Subscription>>> SearchSubscription(string searchString, PaginationFilter paginationFilter);
    }
}
