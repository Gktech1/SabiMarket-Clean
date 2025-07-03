using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Infrastructure.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer> GetCustomerById(string id, bool trackChanges);
        Task<Customer> GetCustomerWithApplicationUser(string id, bool trackChanges);
        Task<Customer> GetCustomerByUserId(string userId, bool trackChanges);
        Task<Customer> GetCustomerDetails(string userId);
        Task<PaginatorDto<IEnumerable<Customer>>> GetCustomersWithPagination(
            PaginationFilter paginationFilter, bool trackChanges, string? searchString);
        Task<bool> CustomerExists(string userId);
        void CreateCustomer(Customer customer);
        void UpdateCustomer(Customer customer);
        void DeleteCustomer(Customer customer);
    }
}
