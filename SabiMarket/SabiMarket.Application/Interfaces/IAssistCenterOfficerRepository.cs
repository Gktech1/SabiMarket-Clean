using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.MarketParticipants;
using System.Linq.Expressions;

namespace SabiMarket.Application.Interfaces
{
    public interface IAssistCenterOfficerRepository
    {
        void AddAssistCenterOfficer(AssistCenterOfficer assistCenter);
        void UpdateAssistCenterOfficer(AssistCenterOfficer assistCenter);
        Task<IEnumerable<AssistCenterOfficer>> GetAllAssistCenterOfficer(bool trackChanges);
        Task<PaginatorDto<IEnumerable<AssistCenterOfficer>>> GetAssistantOfficersAsync(
    string chairmanId, PaginationFilter paginationFilter, bool trackChanges);
        Task<AssistCenterOfficer> GetAssistantOfficerByUserIdAsync(string userId, bool trackChanges);
        Task<AssistCenterOfficer> GetByIdAsync(string officerId, bool trackChanges);
        Task<AssistCenterOfficer> GetAssistantOfficerByIdAsync(string officerId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<AssistCenterOfficer>>> GetAssistOfficersAsync(
       Expression<Func<AssistCenterOfficer, bool>> expression,
       PaginationFilter paginationFilter,
       bool trackChanges);
        Task<PaginatorDto<IEnumerable<AssistCenterOfficer>>> SearchAssistOfficersAsync(
              Expression<Func<AssistCenterOfficer, bool>> baseExpression,
              string searchTerm,
              PaginationFilter paginationFilter,
              bool trackChanges);

        Task<AssistCenterOfficer> GetAssistantOfficerWithTraderAsync(string officerId, string traderId, bool trackChanges);
        void DeleteAssistOfficer(AssistCenterOfficer assistCenterOfficer);
    }
}
