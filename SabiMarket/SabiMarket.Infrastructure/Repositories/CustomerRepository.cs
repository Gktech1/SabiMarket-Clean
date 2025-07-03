using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Repositories;
using SabiMarket.Infrastructure.Utilities;

public class CustomerRepository : GeneralRepository<Customer>, ICustomerRepository
{
    private readonly ApplicationDbContext _repositoryContext;

    public CustomerRepository(ApplicationDbContext repositoryContext)
        : base(repositoryContext)
    {
        _repositoryContext = repositoryContext;
    }

    public async Task<Customer> GetCustomerById(string id, bool trackChanges) =>
        await FindByCondition(c => c.Id == id, trackChanges)
            .FirstOrDefaultAsync();

    public async Task<Customer> GetCustomerWithApplicationUser(string id, bool trackChanges) =>
        await FindByCondition(c => c.Id == id, trackChanges)
            .Include(c => c.User)
            .FirstOrDefaultAsync();

    public async Task<Customer> GetCustomerByUserId(string userId, bool trackChanges) =>
        await FindByCondition(c => c.UserId == userId, trackChanges)
            .FirstOrDefaultAsync();

    public async Task<Customer> GetCustomerDetails(string userId)
    {
        var customer = await FindByCondition(c => c.UserId == userId, trackChanges: false)
            .Include(c => c.User)
            .Include(c => c.LocalGovernment)
            .Include(c => c.Orders)
            .Include(c => c.Feedbacks)
            .Include(c => c.WaivedProduct)
            .FirstOrDefaultAsync();
        return customer;
    }

    public async Task<PaginatorDto<IEnumerable<Customer>>> GetCustomersWithPagination(
    PaginationFilter paginationFilter, bool trackChanges, string? searchString)
    {
        try
        {
            var query = _repositoryContext.Customers
                .Include(c => c.User)
                .Include(c => c.LocalGovernment)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(c =>
                    c.User.FirstName.Contains(searchString) ||
                    c.User.LastName.Contains(searchString) ||
                    c.User.Email.Contains(searchString));
            }

            return await query.OrderBy(c => c.User.FirstName).Paginate(paginationFilter);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while retrieving paged customers", ex);
        }
    }



    public async Task<bool> CustomerExists(string userId) =>
        await FindByCondition(c => c.UserId == userId, trackChanges: false)
            .AnyAsync();

    public void CreateCustomer(Customer customer) =>
        Create(customer);

    public void UpdateCustomer(Customer customer) =>
        Update(customer);

    public void DeleteCustomer(Customer customer) =>
        Delete(customer);
}