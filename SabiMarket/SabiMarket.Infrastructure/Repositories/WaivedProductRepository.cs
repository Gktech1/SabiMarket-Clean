using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class WaivedProductRepository : GeneralRepository<WaivedProduct>, IWaivedProductRepository
    {
        public WaivedProductRepository(ApplicationDbContext context) : base(context) { }

        public void AddWaivedProduct(WaivedProduct product) => Create(product);

        public async Task<IEnumerable<WaivedProduct>> GetAllWaivedProductForExport(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

        public async Task<WaivedProduct> GetWaivedProductById(string id, bool trackChanges) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync();

        public async Task<PaginatorDto<IEnumerable<WaivedProduct>>> GetPagedWaivedProduct(string categoryId, PaginationFilter paginationFilter)
        {
            if (categoryId is not null)
            {
                return await FindAll(false)
                            .Where(l => l.ProductCategoryId.ToLower() == categoryId.ToLower())
                            .Paginate(paginationFilter);
            }
            else
            {
                return await FindAll(false)
                           .Paginate(paginationFilter);
            }
        }

        public async Task<PaginatorDto<IEnumerable<WaivedProduct>>> SearchWaivedProduct(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a => a.ProductName.Contains(searchString))
                           .Paginate(paginationFilter);
        }
    }
}
