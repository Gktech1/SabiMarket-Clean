using AutoMapper;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Advertisement;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.DTOs;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.AdvertisementModule;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using static RoleResponseDto;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LevyPayment, LevyInfoResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName))
            .ForMember(dest => dest.MarketAddress, opt => opt.MapFrom(src => src.Market.Location))
            .ForMember(dest => dest.MarketType, opt => opt.MapFrom(src => "")) // Add proper mapping if MarketType exists
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.PaymentFrequencyDays, opt => opt.MapFrom(src => ConvertPeriodToDays(src.Period)))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => "")) // Map if needed
            .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => "")) // Map if needed
            .ForMember(dest => dest.TraderOccupancy, opt => opt.MapFrom(src => "")) // Add proper mapping if added
            .ForMember(dest => dest.ActiveTradersCount, opt => opt.MapFrom(src => src.Market.TotalTraders))
            .ForMember(dest => dest.PaidTradersCount, opt => opt.MapFrom(src =>
                src.Market.PaymentTransactions))
            .ForMember(dest => dest.DefaultersTradersCount, opt => opt.MapFrom(src =>
                src.Market.TotalTraders - src.Market.PaymentTransactions))
            .ForMember(dest => dest.ExpectedDailyRevenue, opt => opt.MapFrom(src =>
                src.Amount * src.Market.TotalTraders))
            .ForMember(dest => dest.ActualDailyRevenue, opt => opt.MapFrom(src =>
                src.Market.TotalRevenue));

        CreateMap<CreateChairmanRequestDto, Chairman>()
          .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
          .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
          .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
          .ForMember(dest => dest.User, opt => opt.Ignore());

            // Add this missing mapping for LevySetupResponseDto
            CreateMap<LevyPayment, LevySetupResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TraderName, opt => opt.MapFrom(src => src.Trader != null ? src.Trader.TraderName : "")) // Assuming Trader has FullName property
                .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market != null ? src.Market.MarketName : ""))
                .ForMember(dest => dest.TraderOccupancy, opt => opt.MapFrom(src => "")) // Map to appropriate property if available
                .ForMember(dest => dest.PaymentFrequencyDays, opt => opt.MapFrom(src => ConvertPeriodToDays(src.Period)))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.LastUpdatedBy, opt => opt.MapFrom(src => src.UpdatedAt));

        CreateMap<LevySetup, LevySetupResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market != null ? src.Market.MarketName : "")) // You'll need to include Market navigation property or resolve this separately
                .ForMember(dest => dest.TraderOccupancy, opt => opt.MapFrom(src => src.OccupancyType.ToString()))
                .ForMember(dest => dest.PaymentFrequencyDays, opt => opt.MapFrom(src => (int)src.PaymentFrequency))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.LastUpdatedBy, opt => opt.MapFrom(src => src.UpdatedAt));



        CreateMap<Chairman, ChairmanResponseDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
    src.User != null
        ? string.IsNullOrEmpty(src.FullName) ? $"{src.User.FirstName} {src.User.LastName}" : src.FullName
                    : string.Empty))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User != null ? src.User.PhoneNumber : string.Empty))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User != null && src.User.IsActive))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email ?? string.Empty))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber ?? string.Empty))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User.IsActive))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.LocalGovernmentId, opt => opt.MapFrom(src => src.LocalGovernmentId))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.DefaultPassword, opt => opt.Ignore());

        CreateMap<Chairman, ChairmanResponseDto>();

        CreateMap<LevyPayment, LevyResponseDto>()
           // Map enum values to string displays
           .ForMember(dest => dest.PaymentPeriod, opt => opt.MapFrom(src => GetPeriodDisplay(src.Period)))
           .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => GetPaymentMethodDisplay(src.PaymentMethod)))
           .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => GetPaymentStatusDisplay(src.PaymentStatus)))
           // Handle potential null values
           .ForMember(dest => dest.IncentiveAmount, opt => opt.MapFrom(src => src.IncentiveAmount ?? 0m))
           .ForMember(dest => dest.QRCodeScanned, opt => opt.MapFrom(src => src.QRCodeScanned.ToString()))
           // Direct mappings for other properties
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.ChairmanId, opt => opt.MapFrom(src => src.ChairmanId))
           .ForMember(dest => dest.TraderId, opt => opt.MapFrom(src => src.TraderId))
           .ForMember(dest => dest.GoodBoyId, opt => opt.MapFrom(src => src.GoodBoyId))
           .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
           .ForMember(dest => dest.TransactionReference, opt => opt.MapFrom(src => src.TransactionReference))
           .ForMember(dest => dest.HasIncentive, opt => opt.MapFrom(src => src.HasIncentive))
           .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.PaymentDate))
           .ForMember(dest => dest.CollectionDate, opt => opt.MapFrom(src => src.CollectionDate))
           .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));

        // New mappings for LocalGovernment
        CreateMap<LocalGovernment, LocalGovernmentResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.LGA, opt => opt.MapFrom(src => src.LGA))
            .ForMember(dest => dest.CurrentRevenue, opt => opt.MapFrom(src => src.CurrentRevenue));

        // Map ApplicationUser to UserDto
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.ProfileImageUrl))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt));

        // Map for LocalGovernmentWithUsersResponseDto
        CreateMap<(ApplicationUser User, LocalGovernment LocalGovernment), LocalGovernmentWithUsersResponseDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.LocalGovernment, opt => opt.MapFrom(src => src.LocalGovernment));

        CreateMap<Trader, TraderResponseDto>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))

        // CRITICAL FIX: Properly handle User navigation property with null checks
        .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
            src.User != null
                ? $"{src.User.FirstName ?? ""} {src.User.LastName ?? ""}".Trim()
                : string.Empty))

        // CRITICAL FIX: Add null checks for User properties
        .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src =>
            src.User != null ? src.User.PhoneNumber ?? string.Empty : string.Empty))

        .ForMember(dest => dest.Email, opt => opt.MapFrom(src =>
            src.User != null ? src.User.Email ?? string.Empty : string.Empty))

        .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId ?? string.Empty))

        // CRITICAL FIX: Market entity has MarketName property, NOT Name
        .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src =>
            src.Market != null ? src.Market.MarketName ?? string.Empty : src.MarketName ?? string.Empty))

        // CRITICAL FIX: Add null check for Gender
        .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
            src.User != null ? src.User.Gender ?? string.Empty : string.Empty))

        .ForMember(dest => dest.IdentityNumber, opt => opt.MapFrom(src => src.TIN ?? string.Empty))
        .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.BusinessName ?? string.Empty))

        .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src =>
            src.User != null ? src.User.ProfileImageUrl ?? string.Empty : string.Empty))

        .ForMember(dest => dest.TraderName, opt => opt.MapFrom(src => src.TraderName ?? string.Empty))
        .ForMember(dest => dest.BusinessType, opt => opt.MapFrom(src => src.BusinessType ?? string.Empty))

       // CRITICAL FIX: BuildingTypes was showing object type instead of formatted string
       /* .ForMember(dest => dest.BuildingTypes, opt => opt.MapFrom(src =>
            src.BuildingTypes ?? string.Empty))*/
       .ForMember(dest => dest.BuildingTypes, opt => opt.MapFrom(src =>
            src.BuildingTypes != null && src.BuildingTypes.Any()
                ? string.Join(", ", src.BuildingTypes.Select(bt => GetEnumDisplayName(bt.BuildingType)))
                : string.Empty))


        // ALSO FIX: BusinessType enum to string conversion
        .ForMember(dest => dest.BusinessType, opt => opt.MapFrom(src =>
            src.BusinessType != null ? src.BusinessType.ToString() : string.Empty))

        // CRITICAL FIX: DateAdded showing default date - ensure CreatedAt is properly set
        .ForMember(dest => dest.DateAdded, opt => opt.MapFrom(src =>
            src.CreatedAt != default(DateTime) ? src.CreatedAt : DateTime.Now))

        .ForMember(dest => dest.QRCode, opt => opt.MapFrom(src => src.QRCode ?? string.Empty))
        .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));


        CreateMap<Trader, TraderDetailsDto>()
            .IncludeBase<Trader, TraderResponseDto>()
            .ForMember(dest => dest.TraderIdentityNumber, opt => opt.MapFrom(src => src.TIN))
            .ForMember(dest => dest.TraderPhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.DateAddedFormatted, opt => opt.MapFrom(src =>
                src.CreatedAt.ToString("MMM dd, yyyy, hh:mm tt")))
            .ForMember(dest => dest.HasQRCode, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.QRCode)))
            .ForMember(dest => dest.QRCodeImageUrl, opt => opt.MapFrom(src => src.QRCode));

        // ADDITIONAL RECOMMENDATIONS:

        // 1. ADD CONDITIONAL MAPPING FOR BETTER NULL SAFETY:
        CreateMap<Trader, TraderResponseDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                src.User != null
                    ? $"{src.User.FirstName ?? ""} {src.User.LastName ?? ""}".Trim()
                    : string.Empty))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src =>
                src.Market != null ? src.Market.MarketName : src.MarketName));

       /* CreateMap<Trader, TraderResponseDto>()
          .ForMember(dest => dest.IdentityNumber, opt => opt.MapFrom(src => src.TIN));

        CreateMap<Trader, TraderDetailsDto>()
            .IncludeBase<Trader, TraderResponseDto>()
            .ForMember(dest => dest.TraderIdentityNumber, opt => opt.MapFrom(src => src.TIN))
            .ForMember(dest => dest.TraderIdentityNumber, opt => opt.MapFrom(src => src.TIN))
            .ForMember(dest => dest.TraderPhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.DateAddedFormatted, opt => opt.MapFrom(src => src.CreatedAt.ToString("MMM dd, yyyy, hh:mm tt")))
            .ForMember(dest => dest.HasQRCode, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.QRCode)))
            .ForMember(dest => dest.QRCodeImageUrl, opt => opt.MapFrom(src => src.QRCode));
*/
        // Map for UsersByLocalGovernmentResponseDto
        CreateMap<(ApplicationUser User, LocalGovernment LocalGovernment), UsersByLocalGovernmentResponseDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.LocalGovernment, opt => opt.MapFrom(src => src.LocalGovernment));

        // Map for LocalGovernmentResponseDto
        CreateMap<LocalGovernment, LocalGovernmentResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.LGA, opt => opt.MapFrom(src => src.LGA))
            .ForMember(dest => dest.CurrentRevenue, opt => opt.MapFrom(src => src.CurrentRevenue));

        CreateMap<Report, ReportExportDto>()
               .ForMember(dest => dest.TotalMarkets, opt => opt.MapFrom(src => src.MarketCount))
               .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.TotalRevenueGenerated))
               .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
               .ForMember(dest => dest.TotalCaretakers, opt => opt.MapFrom(src => src.TotalCaretakers))
               .ForMember(dest => dest.TraderComplianceRate, opt => opt.MapFrom(src => src.ComplianceRate))
               .ForMember(dest => dest.TotalTransactions, opt => opt.MapFrom(src => src.PaymentTransactions))
               .ForMember(dest => dest.MarketDetails, opt => opt.Ignore())
               .ForMember(dest => dest.RevenueByPaymentMethod, opt => opt.Ignore())
               .ForMember(dest => dest.TransactionsByMarket, opt => opt.Ignore())
               .ForMember(dest => dest.RevenueByMonth, opt => opt.Ignore())
               .ForMember(dest => dest.LGADetails, opt => opt.Ignore())
               .ForMember(dest => dest.ComplianceByMarket, opt => opt.Ignore())
               .AfterMap((src, dest, context) =>
               {
                   // For single-report mapping, we need to manually create the collections
                   // This will be overridden when mapping collections with custom methods
                   if (dest.MarketDetails == null)
                   {
                       dest.MarketDetails = new List<ReportExportDto.MarketSummary>();

                       // Only add market details if we have market data
                       if (!string.IsNullOrEmpty(src.MarketId))
                       {
                           dest.MarketDetails.Add(new ReportExportDto.MarketSummary
                           {
                               MarketId = src.MarketId,
                               MarketName = src.MarketName,
                               Location = src.Market?.Location,
                               LGAName = src.Market?.LocalGovernment?.Name,
                               TotalTraders = src.TotalTraders,
                               Revenue = src.TotalLevyCollected,
                               ComplianceRate = src.ComplianceRate,
                               TransactionCount = src.PaymentTransactions
                           });
                       }
                   }
               });
        // Ensure collection mapping
        CreateMap<IEnumerable<Chairman>, IEnumerable<ChairmanResponseDto>>().ConvertUsing((src, dest, context) =>
            src.Select(chairman => context.Mapper.Map<ChairmanResponseDto>(chairman)));
        CreateMap<IEnumerable<Report>, ReportExportDto>()
                       .ForMember(dest => dest.MarketDetails, opt => opt.MapFrom((src, _, __, context) => MapMarketDetails(src)))
                       .ForMember(dest => dest.RevenueByMonth, opt => opt.MapFrom((src, _, __, context) => MapRevenueByMonth(src)))
                       .ForMember(dest => dest.ComplianceByMarket, opt => opt.MapFrom((src, _, __, context) => MapComplianceByMarket(src)))
                       .ForMember(dest => dest.TotalMarkets, opt => opt.MapFrom(src => src.Select(r => r.MarketId).Distinct().Count()))
                       .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.Sum(r => r.TotalLevyCollected)))
                       .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.Sum(r => r.TotalTraders)))
                       .ForMember(dest => dest.TotalTransactions, opt => opt.MapFrom(src => src.Sum(r => r.PaymentTransactions)))
                       .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Min(r => r.StartDate)))
                       .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Max(r => r.EndDate)))
                       .ForMember(dest => dest.TraderComplianceRate, opt => opt.MapFrom(src =>
                           src.Sum(r => r.TotalTraders) > 0
                               ? (decimal)src.Sum(r => r.CompliantTraders) / src.Sum(r => r.TotalTraders) * 100
                               : 0))
                       .ForMember(dest => dest.DailyAverageRevenue, opt => opt.MapFrom((src, _, __, context) =>
                       {
                           var startDate = src.Min(r => r.StartDate);
                           var endDate = src.Max(r => r.EndDate);
                           var totalDays = (endDate - startDate).Days + 1;
                           var totalRevenue = src.Sum(r => r.TotalLevyCollected);
                           return totalDays > 0 ? totalRevenue / totalDays : 0;
                       }));
        CreateMap<LevySetupRequestDto, LevyPayment>()
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.PaymentFrequencyDays))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(_ => PaymentStatusEnum.Pending)) // Defaulting to Pending
            .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore()) // Assuming it's set later
            .ForMember(dest => dest.TraderId, opt => opt.Ignore()) // Assuming it comes from context
            .ForMember(dest => dest.GoodBoyId, opt => opt.Ignore()) // Assuming it comes from context
            .ForMember(dest => dest.ChairmanId, opt => opt.Ignore()) // Assuming it comes from context
            .ForMember(dest => dest.TransactionReference, opt => opt.Ignore()) // Assuming it's set elsewhere
            .ForMember(dest => dest.PaymentDate, opt => opt.Ignore()) // Set on processing
            .ForMember(dest => dest.CollectionDate, opt => opt.Ignore()) // Set on processing
            .ForMember(dest => dest.Notes, opt => opt.Ignore()) // Optional
            .ForMember(dest => dest.QRCodeScanned, opt => opt.Ignore()) // Optional
            .ForMember(dest => dest.HasIncentive, opt => opt.Ignore()) // Optional
            .ForMember(dest => dest.IncentiveAmount, opt => opt.Ignore()); // Optional

        CreateMap<Admin, AdminDashboardStatsDto>()
           .ForMember(dest => dest.RegisteredLGAs, opt => opt.MapFrom(src => src.RegisteredLGAs))
           .ForMember(dest => dest.ActiveChairmen, opt => opt.MapFrom(src => src.ActiveChairmen))
           .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.TotalRevenue));


        // Ensure Market mapping exists
        CreateMap<Market, MarketResponseDto>();

        // Ensure GoodBoy and Trader mappings exist
        CreateMap<GoodBoy, GoodBoyResponseDto>();
        CreateMap<Trader, TraderResponseDto>();

        /*CreateMap<Market, MarketResponseDto>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MarketName))
           .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
           .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
           .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
           .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.MarketCapacity))
           .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId))
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
           .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));*/


        CreateMap<AssistCenterOfficer, AssistantOfficerResponseDto>()
              .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}".Trim()))
              .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
              .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
              .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
              .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName));

        // For CreateAssistantOfficerRequestDto to AssistCenterOfficer mapping
        CreateMap<CreateAssistantOfficerRequestDto, AssistCenterOfficer>()
            // Map the first MarketId from the list if available (for backward compatibility)
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src =>
                src.MarketIds != null && src.MarketIds.Count > 0 ? src.MarketIds[0] : null))
            // Don't try to map MarketIds directly as it's a collection that will be handled separately
            .ForMember(dest => dest.MarketAssignments, opt => opt.Ignore())
            // The rest remains the same
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Market, opt => opt.Ignore())
            .ForMember(dest => dest.Chairman, opt => opt.Ignore())
            .ForMember(dest => dest.LocalGovernment, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.ChairmanId, opt => opt.Ignore())
            .ForMember(dest => dest.LocalGovernmentId, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // For UpdateAssistantOfficerRequestDto to AssistCenterOfficer mapping
        CreateMap<UpdateAssistantOfficerRequestDto, AssistCenterOfficer>()
            // Map the first MarketId from the list if available (for backward compatibility)
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src =>
                src.MarketIds != null && src.MarketIds.Count > 0 ? src.MarketIds[0] : null))
            // Don't try to map MarketIds directly as it's a collection that will be handled separately
            .ForMember(dest => dest.MarketAssignments, opt => opt.Ignore())
            // The rest remains the same
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Market, opt => opt.Ignore())
            .ForMember(dest => dest.Chairman, opt => opt.Ignore())
            .ForMember(dest => dest.LocalGovernment, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.ChairmanId, opt => opt.Ignore())
            .ForMember(dest => dest.LocalGovernmentId, opt => opt.Ignore())
            .ForMember(dest => dest.IsBlocked, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // For AssistCenterOfficer to AssistantOfficerResponseDto mapping
        CreateMap<AssistCenterOfficer, AssistantOfficerResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.User.ProfileImageUrl))
            .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => src.IsBlocked))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            // Map markets from the MarketAssignments collection
            .ForMember(dest => dest.Markets, opt => opt.MapFrom(src =>
                src.MarketAssignments.Select(ma => new MarketDto
                {
                    Id = ma.Market.Id,
                    Name = ma.Market.MarketName
                })))
            .ForMember(dest => dest.DefaultPassword, opt => opt.Ignore()); // Set manually where needed
        CreateMap<AssistCenterOfficer, AssistantOfficerResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : ""))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.Email : ""))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.PhoneNumber : ""))
                .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
                .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src =>
                    src.Market != null ? src.Market.MarketName : ""))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.Gender : ""))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.DefaultPassword, opt => opt.Ignore()); // Set manually in service

        // If you need to map from AssistCenterOfficer to CreateAssistantOfficerRequestDto
        CreateMap<AssistCenterOfficer, AssistantOfficerResponseDto>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                   src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : ""))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src =>
                   src.User != null ? src.User.Email : ""))
               .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src =>
                   src.User != null ? src.User.PhoneNumber : ""))
               .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
               .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src =>
                   src.Market != null ? src.Market.MarketName : ""))
               .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
                   src.User != null ? src.User.Gender : ""))
               .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
               .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
               .ForMember(dest => dest.DefaultPassword, opt => opt.Ignore()); // This is set manually after mapping

        CreateMap<UpdateProfileDto, Chairman>()
           .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailAddress))
           .ForMember(dest => dest.LocalGovernmentId, opt => opt.MapFrom(src => src.LocalGovernmentId))
           .ForMember(dest => dest.User, opt => opt.Ignore())
           .ForMember(dest => dest.Market, opt => opt.Ignore())
           .ForMember(dest => dest.LocalGovernment, opt => opt.Ignore());

        CreateMap<Chairman, UpdateProfileDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.User.Address))
            .ForMember(dest => dest.LocalGovernmentId, opt => opt.MapFrom(src => src.LocalGovernmentId))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.User.ProfileImageUrl));

        CreateMap<AssistCenterOfficer, AssistantOfficerResponseDto>()
          .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}".Trim()))
          .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
          .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
          .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
          .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName));

        CreateMap<CreateLevyRequestDto, LevyPayment>()
           .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
           .ForMember(dest => dest.TraderId, opt => opt.MapFrom(src => src.TraderId))
           .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
           .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period))
           .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
           .ForMember(dest => dest.HasIncentive, opt => opt.MapFrom(src => src.HasIncentive))
           .ForMember(dest => dest.IncentiveAmount, opt => opt.MapFrom(src => src.IncentiveAmount))
           .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
           .ForMember(dest => dest.GoodBoyId, opt => opt.MapFrom(src => src.GoodBoyId))
           .ForMember(dest => dest.CollectionDate, opt => opt.MapFrom(src => src.CollectionDate))
           .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => PaymentStatusEnum.Pending))
           .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => DateTime.UtcNow))
           .ForMember(dest => dest.TransactionReference, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
           .ForMember(dest => dest.Trader, opt => opt.Ignore())
           .ForMember(dest => dest.Market, opt => opt.Ignore())
           .ForMember(dest => dest.GoodBoy, opt => opt.Ignore())
           .ForMember(dest => dest.Chairman, opt => opt.Ignore());

        CreateMap<UpdateLevyRequestDto, LevyPayment>()
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.TraderId, opt => opt.MapFrom(src => src.TraderId))
            .ForMember(dest => dest.GoodBoyId, opt => opt.MapFrom(src => src.GoodBoyId))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Period, opt => opt.Condition(src => src.Period.HasValue))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period.Value))
            .ForMember(dest => dest.PaymentMethod, opt => opt.Condition(src => src.PaymentMethod.HasValue))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.Value))
            .ForMember(dest => dest.PaymentStatus, opt => opt.Condition(src => src.PaymentStatus.HasValue))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.Value))
            .ForMember(dest => dest.HasIncentive, opt => opt.MapFrom(src => src.HasIncentive))
            .ForMember(dest => dest.IncentiveAmount, opt => opt.MapFrom(src => src.IncentiveAmount))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));


        CreateMap<CreateMarketRequestDto, Market>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
              .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.MarketName))
              .ForMember(dest => dest.MarketType, opt => opt.MapFrom(src => src.MarketType))
              .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId))
              .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
              .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => 0))
              .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => 0))
              .ForMember(dest => dest.PaymentTransactions, opt => opt.MapFrom(src => 0))
              .ForMember(dest => dest.ComplianceRate, opt => opt.MapFrom(src => 0))
              .ForMember(dest => dest.CompliantTraders, opt => opt.MapFrom(src => 0))
              .ForMember(dest => dest.NonCompliantTraders, opt => opt.MapFrom(src => 0))
              .ForMember(dest => dest.OccupancyRate, opt => opt.MapFrom(src => 0));

        // UpdateMarketRequestDto -> Market
        CreateMap<UpdateMarketRequestDto, Market>()
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.MarketName))
            .ForMember(dest => dest.MarketType, opt => opt.MapFrom(src => src.MarketType))
            .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Market -> MarketResponseDto
        /*CreateMap<Market, MarketResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MarketName))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
            .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
            .ForMember(dest => dest.ContactPhone, opt => opt.Ignore()) // Adjust if needed
            .ForMember(dest => dest.ContactEmail, opt => opt.Ignore()) // Adjust if needed
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt))
            .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.Caretaker != null ? src.Caretaker.Id : null)); // Map single caretaker ID*/

        CreateMap<Market, MarketResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.MarketName ?? "Unnamed Market"))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location ?? "No Location"))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? "No description available"))
            .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
            .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
            .ForMember(dest => dest.ContactPhone, opt => opt.Ignore())
            .ForMember(dest => dest.ContactEmail, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt))
            .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId));

        CreateMap<Market, MarketRevenueDto>()
           .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.MarketName))
           .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.TotalRevenue))
           .ForMember(dest => dest.PaymentMethods, opt => opt.Ignore()) // This should be calculated separately
           .ForMember(dest => dest.GrowthRate, opt => opt.Ignore())     // This needs calculation
           .ForMember(dest => dest.AverageDaily, opt => opt.Ignore())   // This needs calculation
           .ForMember(dest => dest.AverageMonthly, opt => opt.Ignore());// This needs calculation

        CreateMap<AssistCenterOfficer, AssistantOfficerResponseDto>()
           .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}".Trim()))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
           .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
           .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
           .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName))
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
           .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));


        /* CreateMap<Market, MarketResponseDto>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MarketName))
           .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
           .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
           .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
           .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
           .ForMember(dest => dest.ContactPhone, opt => opt.Ignore()) // Adjust if needed
           .ForMember(dest => dest.ContactEmail, opt => opt.Ignore()) // Adjust if needed
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
           .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt))
           .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId)); // Single caretaker ID mapping*/

        CreateMap<Caretaker, CaretakerResponseDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User.IsActive))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.Market, opt => opt.MapFrom(src => src.Markets.FirstOrDefault()))  // Assuming one Market is related, adjust as needed
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => src.IsBlocked))
            .ForMember(dest => dest.DefaultPassword, opt => opt.Ignore()); // DefaultPassword should be handled separately if needed.

        CreateMap<AssistCenterOfficer, AssistOfficerListDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
                .ForMember(dest => dest.LocalGovernmentId, opt => opt.MapFrom(src => src.LocalGovernmentId))
                .ForMember(dest => dest.UserLevel, opt => opt.MapFrom(src => src.UserLevel))
                .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => src.IsBlocked))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                // The following properties are manually set in the service
                .ForMember(dest => dest.FullName, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
                .ForMember(dest => dest.MarketName, opt => opt.Ignore())
                .ForMember(dest => dest.LocalGovernmentName, opt => opt.Ignore());

        // Map from entity to details DTO
        CreateMap<AssistCenterOfficer, AssistOfficerDetailsDto>()
            .IncludeBase<AssistCenterOfficer, AssistOfficerListDto>()
            .ForMember(dest => dest.ChairmanId, opt => opt.MapFrom(src => src.ChairmanId))
            .ForMember(dest => dest.ChairmanName, opt => opt.Ignore())
            .ForMember(dest => dest.Gender, opt => opt.Ignore());

        // CreateMarketRequestDto to Market
        CreateMap<CreateMarketRequestDto, Market>()
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.MarketName))
            .ForMember(dest => dest.MarketType, opt => opt.MapFrom(src => src.MarketType.ToString()))
            .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            // Set default values for required fields not in DTO
            .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.MarketCapacity, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.OccupancyRate, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.ComplianceRate, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.CompliantTraders, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.NonCompliantTraders, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.PaymentTransactions, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => 0))
            // Ignore navigation properties and other fields that should be set separately
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Traders, opt => opt.Ignore())
            .ForMember(dest => dest.MarketSections, opt => opt.Ignore())
            .ForMember(dest => dest.Chairman, opt => opt.Ignore())
            .ForMember(dest => dest.LocalGovernment, opt => opt.Ignore())
            .ForMember(dest => dest.Caretaker, opt => opt.Ignore());

        CreateMap<Caretaker, CaretakerResponseDto>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
             .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
             .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User != null ? src.User.FirstName : null))
             .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User != null ? src.User.LastName : null))
             .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                 src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : ""))
             .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User != null ? src.User.IsActive : false))
             .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => src.IsBlocked))
             .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
             .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));   // UpdateMarketRequestDto to Market
        CreateMap<UpdateMarketRequestDto, Market>()
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.MarketName))
            .ForMember(dest => dest.MarketType, opt => opt.MapFrom(src => src.MarketType.ToString()))
            .ForMember(dest => dest.CaretakerId, opt => opt.MapFrom(src => src.CaretakerId))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            // Ignore fields that shouldn't be updated through this DTO
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.TotalTraders, opt => opt.Ignore())
            .ForMember(dest => dest.MarketCapacity, opt => opt.Ignore())
            .ForMember(dest => dest.OccupancyRate, opt => opt.Ignore())
            .ForMember(dest => dest.ComplianceRate, opt => opt.Ignore())
            .ForMember(dest => dest.CompliantTraders, opt => opt.Ignore())
            .ForMember(dest => dest.NonCompliantTraders, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentTransactions, opt => opt.Ignore())
            .ForMember(dest => dest.TotalRevenue, opt => opt.Ignore())
            .ForMember(dest => dest.Traders, opt => opt.Ignore())
            .ForMember(dest => dest.MarketSections, opt => opt.Ignore())
            .ForMember(dest => dest.Chairman, opt => opt.Ignore())
            .ForMember(dest => dest.LocalGovernment, opt => opt.Ignore())
            .ForMember(dest => dest.Caretaker, opt => opt.Ignore());

        CreateMap<GoodBoy, GoodBoyResponseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName))
            .ForMember(dest => dest.TraderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TraderOccupancy, opt => opt.MapFrom(src => "Open Space"))
            .ForMember(dest => dest.PaymentFrequency, opt => opt.MapFrom(src => "2 days - N500"))
            .ForMember(dest => dest.LastPaymentDate, opt => opt.MapFrom(src =>
                src.LevyPayments.OrderByDescending(p => p.PaymentDate).FirstOrDefault().PaymentDate))
            .ForMember(dest => dest.LevyPayments, opt => opt.MapFrom(src => src.LevyPayments));

        CreateMap<Market, MarketDetailsDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MarketName))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
            .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
            .ForMember(dest => dest.ContactPhone, opt => opt.Ignore())
            .ForMember(dest => dest.ContactEmail, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.TotalRevenue))
            .ForMember(dest => dest.ComplianceRate, opt => opt.MapFrom(src => src.ComplianceRate))
            .ForMember(dest => dest.Caretakers, opt => opt.MapFrom((src, dest, _, context) =>
            {
                var caretakers = new List<CaretakerResponseDto>();

                // Add the primary caretaker if it exists
                if (src.Caretaker != null)
                {
                    caretakers.Add(context.Mapper.Map<CaretakerResponseDto>(src.Caretaker));
                }

                // Add any additional caretakers
                if (src.AdditionalCaretakers != null && src.AdditionalCaretakers.Any())
                {
                    caretakers.AddRange(src.AdditionalCaretakers.Select(c => context.Mapper.Map<CaretakerResponseDto>(c)));
                }

                return caretakers;
            }))
                .ForMember(dest => dest.Traders, opt => opt.MapFrom(src => src.Traders));

        CreateMap<Trader, TraderResponseDto>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
          .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
              src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : "Unknown"))
          .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src =>
              src.User != null ? src.User.PhoneNumber : null))
          .ForMember(dest => dest.Email, opt => opt.MapFrom(src =>
              src.User != null ? src.User.Email : null))
          .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
              src.User != null ? src.User.Gender : null))
          .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
          .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src =>
              src.Market != null ? src.Market.MarketName : null))
          .ForMember(dest => dest.QRCode, opt => opt.MapFrom(src => src.QRCode))
          .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.BusinessName))
          .ForMember(dest => dest.BusinessType, opt => opt.MapFrom(src => src.BusinessType))
          .ForMember(dest => dest.IdentityNumber, opt => opt.MapFrom(src => src.TIN))
          .ForMember(dest => dest.DateAdded, opt => opt.MapFrom(src => src.CreatedAt))
          .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsActive)); // This line was wrongber(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsActive));

        CreateMap<Caretaker, CaretakerResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User != null ?
                $"{src.User.FirstName} {src.User.LastName}" : "Unknown"))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User != null ?
                src.User.PhoneNumber : null))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ?
                src.User.Email : null))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.User != null ?
                src.User.ProfileImageUrl : null));

        CreateMap<CreateGoodBoyRequestDto, ApplicationUser>()
           .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<CreateGoodBoyRequestDto, GoodBoy>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => StatusEnum.Unlocked));

        CreateMap<GoodBoy, GoodBoyResponseDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.MarketName, opt => opt.MapFrom(src => src.Market.MarketName))
            .ForMember(dest => dest.TraderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TraderOccupancy, opt => opt.MapFrom(src => "Open Space"))
            .ForMember(dest => dest.PaymentFrequency, opt => opt.MapFrom(src => "2 days - N500"))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => 500m))
            .ForMember(dest => dest.LastPaymentDate, opt => opt.MapFrom(src =>
                src.LevyPayments.OrderByDescending(p => p.PaymentDate).FirstOrDefault().PaymentDate));

        CreateMap<ProcessLevyPaymentDto, LevyPayment>()
        .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => DateTime.UtcNow))
        .ForMember(dest => dest.TransactionReference, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
        .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => PaymentStatusEnum.Paid));

        CreateMap<AuditLog, AuditLogResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.UserFullName, opt =>
                opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}".Trim()))
            .ForMember(dest => dest.UserRole, opt => opt.Ignore())  // We'll set this separately
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.Time))
            .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
            .ForMember(dest => dest.Activity, opt => opt.MapFrom(src => src.Activity))
            .ForMember(dest => dest.Module, opt => opt.MapFrom(src => src.Module))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details))
            .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Completed"))
            .ForMember(dest => dest.Browser, opt => opt.Ignore())
            .ForMember(dest => dest.OperatingSystem, opt => opt.Ignore())
            .ForMember(dest => dest.Location, opt => opt.Ignore());

        CreateMap<Chairman, ReportResponseDto>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
               // Since Chairman doesn't have Description, we can set a default
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Office))
               .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.FullName))
               .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.TermStart))
               .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
               .ForMember(dest => dest.ChairmanId, opt => opt.MapFrom(src => src.Id));

        /*CreateMap<Market, MarketDetailsDto>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MarketName))
           .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
           .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
           .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.TotalTraders))
           .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
           .ForMember(dest => dest.ContactPhone, opt => opt.Ignore()) // No matching property in source
           .ForMember(dest => dest.ContactEmail, opt => opt.Ignore()) // No matching property in source
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
           .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
           .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.TotalRevenue))
           .ForMember(dest => dest.ComplianceRate, opt => opt.MapFrom(src => src.ComplianceRate));*/

        CreateMap<LocalGovernment, LGAResponseDto>()
                   .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                   .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                   .ForMember(dest => dest.StateId, opt => opt.MapFrom(src => src.State)) // Assuming State field contains StateId
                   .ForMember(dest => dest.StateName, opt => opt.MapFrom(src => src.State)) // You might need to adjust this if StateName comes from a different property
                   .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.LGA))
                   .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true)) // Set default or map from appropriate property
                   .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => "")) // Map from appropriate property if available
                   .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                   .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => "")) // Map from appropriate property if available
                   .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                   // Statistics
                   .ForMember(dest => dest.TotalMarkets, opt => opt.MapFrom(src => src.Markets.Count))
                   .ForMember(dest => dest.ActiveMarkets, opt => opt.MapFrom(src => src.Markets.Count(m => m.IsActive)))
                   .ForMember(dest => dest.TotalTraders, opt => opt.MapFrom(src => src.Vendors.Count))
                   .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.CurrentRevenue));

        // CaretakerForCreationRequestDto -> Caretaker
        CreateMap<CaretakerForCreationRequestDto, Caretaker>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => new ApplicationUser
            {
                UserName = src.EmailAddress,
                Email = src.EmailAddress,
                PhoneNumber = src.PhoneNumber,
                EmailConfirmed = true
            }))
            .ForMember(dest => dest.MarketId, opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Will be set after user creation
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Markets, opt => opt.Ignore())
            .ForMember(dest => dest.GoodBoys, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedTraders, opt => opt.Ignore())
            .ForMember(dest => dest.Chairman, opt => opt.Ignore())
            .ForMember(dest => dest.LocalGovernment, opt => opt.Ignore());

        CreateMap<Advertisement, AdvertisementResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.User.FirstName + " " + src.Vendor.User.LastName : ""))
                .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.Views != null ? src.Views.Count : 0));

        // Advertisement -> AdvertisementDetailResponseDto
        CreateMap<Advertisement, AdvertisementDetailResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.User.FirstName + " " + src.Vendor.User.LastName : ""))
            .ForMember(dest => dest.AdminName, opt => opt.MapFrom(src => src.Admin != null ? src.Admin.User.FirstName + " " + src.Admin.User.LastName : ""))
            .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.Views != null ? src.Views.Count : 0));

        // AdvertisementLanguage -> AdvertisementLanguageDto
        CreateMap<AdvertisementLanguage, AdvertisementLanguageDto>();

        // AdvertPayment -> AdvertPaymentDto
        CreateMap<AdvertPayment, AdvertPaymentDto>()
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.TransactionReference, opt => opt.MapFrom(src => src.AccountNumber))
            .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<LevyPayment, GoodBoyLevyPaymentResponseDto>()
    .ForMember(dest => dest.TraderName, opt => opt.MapFrom(src => src.Trader.User.FirstName + " " + src.Trader.User.LastName))
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.PaymentStatus.ToString()));

        CreateMap<LevyPayment, GoodBoyLevyPaymentResponseDto>();

        CreateMap<ApplicationRole, RoleResponseDto>();
        CreateMap<RolePermission, RolePermissionDto>();

        //CreateMap<LevyPayment, GoodBoyLevyPaymentResponseDto>();

        CreateMap<Market, MarketResponseDto>();
        CreateMap<GoodBoy, GoodBoyResponseDto>();
        CreateMap<Trader, TraderResponseDto>();


    }

    private int ConvertPeriodToDays(PaymentPeriodEnum period) => period switch
    {
        PaymentPeriodEnum.Daily => 1,
        PaymentPeriodEnum.Weekly => 7,
        PaymentPeriodEnum.Monthly => 30,
        PaymentPeriodEnum.Yearly => 365,
        _ => 1
    };

    private static string GetPeriodDisplay(PaymentPeriodEnum period)
    {
        return period switch
        {
            PaymentPeriodEnum.Daily => "Daily",
            PaymentPeriodEnum.Weekly => "Weekly",
            PaymentPeriodEnum.BiWeekly => "Bi-Weekly",
            PaymentPeriodEnum.Monthly => "Monthly",
            PaymentPeriodEnum.Quarterly => "Quarterly",
            PaymentPeriodEnum.HalfYearly => "Half-Yearly",
            PaymentPeriodEnum.Yearly => "Yearly",
            _ => period.ToString()
        };
    }

    private static string GetPaymentMethodDisplay(PaymenPeriodEnum method)
    {
        return method switch
        {
            PaymenPeriodEnum.Cash => "Cash",
            PaymenPeriodEnum.BankTransfer => "Bank Transfer",
            PaymenPeriodEnum.MobileMoney => "Mobile Money",
            PaymenPeriodEnum.AssistCenter => "Assist Center",
            _ => method.ToString()
        };
    }

    private static string GetPaymentStatusDisplay(PaymentStatusEnum status)
    {
        return status switch
        {
            PaymentStatusEnum.Pending => "Pending",
            PaymentStatusEnum.Paid => "Paid",
            PaymentStatusEnum.Unpaid => "Unpaid",
            PaymentStatusEnum.Failed => "Failed",
            _ => status.ToString()
        };
    }


    private List<ReportExportDto.MarketSummary> MapMarketDetails(IEnumerable<Report> reports)
    {
        var marketDetails = new List<ReportExportDto.MarketSummary>();

        // Group by market to get aggregated stats
        var marketGroups = reports.Where(r => !string.IsNullOrEmpty(r.MarketId))
                                 .GroupBy(r => r.MarketId);

        foreach (var marketGroup in marketGroups)
        {
            var firstReport = marketGroup.First();

            marketDetails.Add(new ReportExportDto.MarketSummary
            {
                MarketId = firstReport.MarketId,
                MarketName = firstReport.MarketName,
                Location = firstReport.Market?.Location,
                LGAName = firstReport.Market?.LocalGovernment?.Name,
                TotalTraders = marketGroup.Sum(r => r.TotalTraders),
                Revenue = marketGroup.Sum(r => r.TotalLevyCollected),
                ComplianceRate = marketGroup.Any(r => r.TotalTraders > 0)
                    ? marketGroup.Sum(r => r.CompliantTraders) / (decimal)marketGroup.Sum(r => r.TotalTraders) * 100
                    : 0,
                TransactionCount = marketGroup.Sum(r => r.PaymentTransactions)
            });
        }

        return marketDetails;
    }

    private List<ReportExportDto.MarketMonthlyRevenue> MapRevenueByMonth(IEnumerable<Report> reports)
    {
        var revenueByMonth = new List<ReportExportDto.MarketMonthlyRevenue>();

        // Group by market
        var marketGroups = reports.Where(r => !string.IsNullOrEmpty(r.MarketId))
                                 .GroupBy(r => r.MarketId);

        foreach (var marketGroup in marketGroups)
        {
            var firstReport = marketGroup.First();
            var monthlyData = new List<ReportExportDto.MonthlyData>();

            // Group by year and month
            var monthGroups = marketGroup.GroupBy(r => new { r.Year, r.Month });

            foreach (var monthGroup in monthGroups)
            {
                var monthName = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1)
                                    .ToString("MMM");

                monthlyData.Add(new ReportExportDto.MonthlyData
                {
                    Month = monthName,
                    MonthNumber = monthGroup.Key.Month,
                    Year = monthGroup.Key.Year,
                    Revenue = monthGroup.Sum(r => r.MonthlyRevenue),
                    TransactionCount = monthGroup.Sum(r => r.PaymentTransactions)
                });
            }

            revenueByMonth.Add(new ReportExportDto.MarketMonthlyRevenue
            {
                MarketId = firstReport.MarketId,
                MarketName = firstReport.MarketName,
                MonthlyData = monthlyData.OrderBy(m => m.Year).ThenBy(m => m.MonthNumber).ToList()
            });
        }

        return revenueByMonth;
    }

    private List<ReportExportDto.MarketCompliance> MapComplianceByMarket(IEnumerable<Report> reports)
    {
        var complianceByMarket = new List<ReportExportDto.MarketCompliance>();

        // Group by market
        var marketGroups = reports.Where(r => !string.IsNullOrEmpty(r.MarketId))
                                 .GroupBy(r => r.MarketId);

        foreach (var marketGroup in marketGroups)
        {
            var firstReport = marketGroup.First();
            var totalTraders = marketGroup.Sum(r => r.TotalTraders);
            var compliantTraders = marketGroup.Sum(r => r.CompliantTraders);

            complianceByMarket.Add(new ReportExportDto.MarketCompliance
            {
                MarketId = firstReport.MarketId,
                MarketName = firstReport.MarketName,
                CompliancePercentage = totalTraders > 0 ? (decimal)compliantTraders / totalTraders * 100 : 0,
                CompliantTraders = compliantTraders,
                NonCompliantTraders = totalTraders - compliantTraders
            });
        }

        return complianceByMarket;
    }

    public static string GetEnumDisplayName(Enum enumValue)
    {
        return enumValue
            .GetType()
            .GetMember(enumValue.ToString())
            .FirstOrDefault()?
            .GetCustomAttribute<DisplayAttribute>()?
            .Name ?? enumValue.ToString();
    }

}


