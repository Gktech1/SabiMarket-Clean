using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Application.IRepositories
{
    public interface IVendorRepository : IGeneralRepository<Vendor>
    {
        Task<Vendor> GetVendorById(string id, bool trackChanges);
        Task<Vendor> GetVendorWithApplicationUser(string id, bool trackChanges);
        Task<Vendor> GetVendorByUserId(string userId, bool trackChanges);

        Task<Vendor> GetVendorDetails(string userId);
        Task<PaginatorDto<IEnumerable<Vendor>>> GetVendorsWithPagination(
            PaginationFilter paginationFilter, bool trackChanges);
        Task<bool> VendorExists(string userId);
        void CreateVendor(Vendor vendor);
        void UpdateVendor(Vendor vendor);
        void DeleteVendor(Vendor vendor);
    }

}
