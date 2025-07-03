using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Application.IRepositories
{
    public interface IWaivedProductRepository
    {
        void AddWaivedProduct(WaivedProduct product);
        Task<IEnumerable<WaivedProduct>> GetAllWaivedProductForExport(bool trackChanges);
        Task<PaginatorDto<IEnumerable<WaivedProduct>>> GetPagedWaivedProduct(string category, PaginationFilter paginationFilter);
        Task<WaivedProduct> GetWaivedProductById(string id, bool trackChanges);
        Task<PaginatorDto<IEnumerable<WaivedProduct>>> SearchWaivedProduct(string searchString, PaginationFilter paginationFilter);
    }
}
