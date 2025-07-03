using Microsoft.EntityFrameworkCore.Storage;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories.SowFoodIRepositories;
using SabiMarket.Infrastructure.Repositories;
using System.Data;

namespace SabiMarket.Application.IRepositories
{
    public interface IRepositoryManager
    {
        public ILevyPaymentRepository LevyPaymentRepository { get; }
        public IMarketRepository MarketRepository { get; }
        public IWaivedProductRepository WaivedProductRepository { get; }
        public ISubscriptionRepository SubscriptionRepository { get; }
        public ISubscriptionPlanRepository SubscriptionPlanRepository { get; }

        public ICaretakerRepository CaretakerRepository { get; }

        public IGoodBoyRepository GoodBoyRepository { get; }

        public ITraderRepository TraderRepository { get; }

        public ILocalGovernmentRepository LocalGovernmentRepository { get; }

        public IVendorRepository VendorRepository { get; }

        public IChairmanRepository ChairmanRepository { get; }
        public IAssistCenterOfficerRepository AssistCenterOfficerRepository { get; }
        public IAuditLogRepository AuditLogRepository { get; }

        public IReportRepository ReportRepository { get; }

        public IAdminRepository AdminRepository { get; }
        public ISowFoodStaffRepository StaffRepository { get; }

        public IAdvertisementRepository AdvertisementRepository { get; }    
        public ICustomerRepository CustomerRepository { get; }

        public IOfficerMarketAssignmentRepository OfficerMarketAssignmentRepository { get; }
        /// <summary>
        /// Begins a new database transaction with the default isolation level (ReadCommitted)
        /// </summary>
        /// <returns>The transaction object</returns>
        Task<IDbContextTransaction> BeginTransactionAsync();

        /// <summary>
        /// Begins a new database transaction with the specified isolation level
        /// </summary>
        /// <param name="isolationLevel">The isolation level for the transaction</param>
        /// <returns>The transaction object</returns>
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel);

        Task SaveChangesAsync();
    }
}
