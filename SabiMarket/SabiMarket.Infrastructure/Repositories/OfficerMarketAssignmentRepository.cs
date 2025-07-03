
using Microsoft.EntityFrameworkCore;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Repositories;

public class OfficerMarketAssignmentRepository : GeneralRepository<OfficerMarketAssignment>, IOfficerMarketAssignmentRepository
{
    public OfficerMarketAssignmentRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public void AddAssignment(OfficerMarketAssignment assignment)
    {
        Create(assignment);
    }

    public void RemoveAssignment(OfficerMarketAssignment assignment)
    {
        Delete(assignment);
    }

    public async Task<List<OfficerMarketAssignment>> GetAssignmentsByOfficerId(string officerId)
    {
        return await FindByCondition(a => a.AssistCenterOfficerId == officerId, false)
            .Include(a => a.Market)
            .ToListAsync();
    }

    public async Task<bool> HasAssignment(string officerId, string marketId)
    {
        return await FindByCondition(a => a.AssistCenterOfficerId == officerId && a.MarketId == marketId, false)
            .AnyAsync();
    }
}