using SabiMarket.Application.IRepositories;

namespace SabiMarket.Application.Interfaces
{
    public interface IGoodBoyRepository : IGeneralRepository<GoodBoy>
    {
        void AddGoodBoy(GoodBoy goodBoy);
        void UpdateGoodBoy(GoodBoy goodBoy);
        Task<IEnumerable<GoodBoy>> GetAllAssistCenterOfficer(bool trackChanges);
        Task<GoodBoy> GetGoodBoyByUserId(string userId, bool trackChanges = false);
        Task<GoodBoy> GetGoodBoyById(string id, bool trackChanges = false);
        Task<IEnumerable<GoodBoy>> GetGoodBoysByMarketId(string marketId, bool trackChanges = false);
        void DeleteGoodBoy(GoodBoy goodBoy);
        Task<bool> GoodBoyExists(string id);
    }
}
