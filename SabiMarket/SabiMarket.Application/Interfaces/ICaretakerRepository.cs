using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.MarketParticipants;

namespace SabiMarket.Application.Interfaces
{
    public interface ICaretakerRepository
    {
        IQueryable<Caretaker> GetCaretakersQuery();
        Task<Caretaker> GetCaretakerById(string userId, bool trackChanges);
        Task<Caretaker> GetCaretakerByMarketId(string marketId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<Caretaker>>> GetCaretakersWithPagination(PaginationFilter paginationFilter, bool trackChanges);
        Task<bool> CaretakerExists(string chairmanId, string marketId);
        Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPayments(string caretakerId, PaginationFilter paginationFilter, bool trackChanges);
        Task<LevyPayment> GetLevyPaymentDetails(string levyId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<Caretaker>>> GetCaretakersAsync(
           string chairmanId, PaginationFilter paginationFilter, bool trackChanges);

        Task<IEnumerable<Caretaker>> GetAllCaretakersByUserId(string userId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<GoodBoy>>> GetGoodBoys(string caretakerId, PaginationFilter paginationFilter, bool trackChanges);
        void CreateCaretaker(Caretaker caretaker);
        void UpdateCaretaker(Caretaker updatecaretaker);
        void DeleteCaretaker(Caretaker caretaker);
        Task<int> GetCaretakerCountAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Caretaker>> GetAllCaretakers(bool trackChanges);
        Task<bool> ExistsAsync(string id);
        Task<Caretaker> GetCaretakerByLocalGovernmentId(string LGAId, bool trackChanges);
        Task<bool> CaretakerExistsAsync(string useriId);
    }
}
