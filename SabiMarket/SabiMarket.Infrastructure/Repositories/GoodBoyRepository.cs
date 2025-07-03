using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.Interfaces;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Repositories;
public class GoodBoyRepository : GeneralRepository<GoodBoy>, IGoodBoyRepository
{
    private readonly ApplicationDbContext _context;

    public GoodBoyRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public void AddGoodBoy(GoodBoy goodBoy) => Create(goodBoy);

    public void UpdateGoodBoy(GoodBoy goodBoy) => Update(goodBoy);

    public async Task<IEnumerable<GoodBoy>> GetAllAssistCenterOfficer(bool trackChanges) =>
        await FindAll(trackChanges).ToListAsync();

    public async Task<GoodBoy> GetGoodBoyByUserId(string userId, bool trackChanges = false) =>
       await FindByCondition(g => g.UserId == userId, trackChanges)
           .Include(g => g.User)
           .Include(g => g.Market)
           .Include(g => g.LevyPayments)
           .FirstOrDefaultAsync();

    public async Task<GoodBoy> GetGoodBoyById(string id, bool trackChanges = false) =>
        await FindByCondition(g => g.Id == id, trackChanges)
            .Include(g => g.User)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<GoodBoy>> GetGoodBoysByMarketId(string marketId, bool trackChanges = false) =>
        await FindByCondition(g => g.MarketId == marketId, trackChanges)
            .ToListAsync();

    public void DeleteGoodBoy(GoodBoy goodBoy) => Delete(goodBoy);

    public async Task<bool> GoodBoyExists(string id) =>
        await FindByCondition(g => g.Id == id, false).AnyAsync();
}