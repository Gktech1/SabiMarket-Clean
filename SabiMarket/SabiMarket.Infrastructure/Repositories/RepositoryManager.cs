using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.IRepositories.SowFoodIRepositories;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Repositories.SowFoodLinkedRepositories;
using System.Data;

namespace SabiMarket.Infrastructure.Repositories
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly ApplicationDbContext _context;
        private readonly Lazy<ILevyPaymentRepository> _levyPaymentRepository;
        private readonly Lazy<IMarketRepository> _marketRepository;
        private readonly Lazy<IWaivedProductRepository> _waivedProductRepository;
        private readonly Lazy<ISubscriptionPlanRepository> _subscriptionPlanRepository;
        private readonly Lazy<ISubscriptionRepository> _subscriptionRepository;
        private readonly Lazy<ICaretakerRepository> _cretakerRepository;
        private readonly Lazy<IGoodBoyRepository> _goodboyRepository;
        private readonly Lazy<ITraderRepository> _traderRepository;
        private readonly Lazy<ILocalGovernmentRepository> _localgovernmentRepository;
        private readonly Lazy<IVendorRepository> _vendorRepository;
        private readonly Lazy<IChairmanRepository> _chairmanRepository;
        private readonly Lazy<IAssistCenterOfficerRepository> _assistofficerRepository;
        private readonly Lazy<IAuditLogRepository> _auditlogRepository;
        private readonly Lazy<IReportRepository> _reportRepository;
        private readonly Lazy<IAdminRepository> _adminRepository;
        private readonly Lazy<ISowFoodStaffRepository> _staffRepository;
        private readonly Lazy<ICustomerRepository> _customerRepository;
        private readonly Lazy<IAdvertisementRepository> _advertisementRepository;
        private readonly Lazy<IOfficerMarketAssignmentRepository> _officerMarketAssignmentRepository;




        public RepositoryManager(ApplicationDbContext context)
        {
            _context = context;
            _levyPaymentRepository = new Lazy<ILevyPaymentRepository>(() => new LevyPaymentRepository(_context));
            _marketRepository = new Lazy<IMarketRepository>(() => new MarketRepository(_context));
            _waivedProductRepository = new Lazy<IWaivedProductRepository>(() => new WaivedProductRepository(_context));
            _subscriptionRepository = new Lazy<ISubscriptionRepository>(() => new SubscriptionRepository(_context));
            _subscriptionPlanRepository = new Lazy<ISubscriptionPlanRepository>(() => new SubscriptionPlanRepository(_context));
            _cretakerRepository = new Lazy<ICaretakerRepository>(() => new CaretakerRepository(_context));
            _goodboyRepository = new Lazy<IGoodBoyRepository>(() => new GoodBoyRepository(_context));
            _traderRepository = new Lazy<ITraderRepository>(() => new TraderRepository(_context));
            _localgovernmentRepository = new Lazy<ILocalGovernmentRepository>(() => new LocalGovernmentRepository(_context));
            _vendorRepository = new Lazy<IVendorRepository>(() => new VendorRepository(_context));
            _chairmanRepository = new Lazy<IChairmanRepository>(() => new ChairmanRepository(_context));
            _assistofficerRepository = new Lazy<IAssistCenterOfficerRepository>(() => new AssistCenterOfficerRepository(_context));
            _auditlogRepository = new Lazy<IAuditLogRepository>(() => new AuditLogRepository(_context));
            _reportRepository = new Lazy<IReportRepository>(() => new ReportRepository(_context));
            _adminRepository = new Lazy<IAdminRepository>(() => new AdminRepository(_context));
            _staffRepository = new Lazy<ISowFoodStaffRepository>(() => new SowFoodStaffRepository(_context));
            _advertisementRepository = new Lazy<IAdvertisementRepository>(() => new AdvertisementRepository(_context));
            _customerRepository = new Lazy<ICustomerRepository>(() => new CustomerRepository(_context));
            _officerMarketAssignmentRepository = new Lazy<IOfficerMarketAssignmentRepository>(() => new OfficerMarketAssignmentRepository(_context));

        }

        public ILevyPaymentRepository LevyPaymentRepository => _levyPaymentRepository.Value;
        public IMarketRepository MarketRepository => _marketRepository.Value;
        public IWaivedProductRepository WaivedProductRepository => _waivedProductRepository.Value;
        public ISubscriptionRepository SubscriptionRepository => _subscriptionRepository.Value;
        public ISubscriptionPlanRepository SubscriptionPlanRepository => _subscriptionPlanRepository.Value;
        public ICaretakerRepository CaretakerRepository => _cretakerRepository.Value;

        public IGoodBoyRepository GoodBoyRepository => _goodboyRepository.Value;

        public ITraderRepository TraderRepository => _traderRepository.Value;

        public ILocalGovernmentRepository LocalGovernmentRepository => _localgovernmentRepository.Value;

        public IChairmanRepository ChairmanRepository => _chairmanRepository.Value;

        public IVendorRepository VendorRepository => _vendorRepository.Value;

        public IAssistCenterOfficerRepository AssistCenterOfficerRepository => _assistofficerRepository.Value;

        public IAuditLogRepository AuditLogRepository => _auditlogRepository.Value;

        public IReportRepository ReportRepository => _reportRepository.Value;
        public IAdminRepository AdminRepository => _adminRepository.Value;
        public ISowFoodStaffRepository StaffRepository => _staffRepository.Value;
        public IAdvertisementRepository AdvertisementRepository => _advertisementRepository.Value;
        public ICustomerRepository CustomerRepository => _customerRepository.Value;
        public IOfficerMarketAssignmentRepository OfficerMarketAssignmentRepository => _officerMarketAssignmentRepository.Value;
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
        /// <summary>
        /// Begins a new database transaction
        /// </summary>
        /// <returns>The transaction object</returns>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Begins a new database transaction with the specified isolation level
        /// </summary>
        /// <param name="isolationLevel">The isolation level for the transaction</param>
        /// <returns>The transaction object</returns>
        public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            return await _context.Database.BeginTransactionAsync(isolationLevel);
        }
    }
}
