using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.API.Services.Notifications;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Advertisement;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.IServices;
using SabiMarket.Application.Services;
using SabiMarket.Application.Services.Interfaces;
using SabiMarket.Application.Validators;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Infrastructure.Configuration;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Helpers;
using SabiMarket.Infrastructure.Repositories;
using SabiMarket.Infrastructure.Services;
using SabiMarket.Infrastructure.Services.Notification;
using SabiMarket.Infrastructure.Services.Payment;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }



    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Identity configuration
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 10;
            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddRoleManager<RoleManager<ApplicationRole>>();

        // Register Services
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IValidator<RegistrationRequestDto>, RegistrationRequestValidator>();
        services.AddScoped<IValidator<LoginRequestDto>, LoginRequestValidator>();
        // services.AddTransient<IEmailService, FreeEmailService>();
        services.AddSingleton<IEmailService, MailjetEmailService>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        services.AddScoped<IServiceManager, ServiceManager>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IValidator<TokenRequestDto>, TokenRequestValidator>();
        services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordValidator>();
        services.AddScoped<IValidator<CreateGoodBoyDto>, CreateGoodBoyValidator>();
        services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordValidator>();
        services.AddScoped<IValidator<CreateAdminRequestDto>, CreateAdminRequestValidator>();
        services.AddScoped<IValidator<UpdateAdminProfileDto>, UpdateAdminProfileValidator>();
        services.AddScoped<IValidator<CreateRoleRequestDto>, CreateRoleRequestValidator>();
        services.AddScoped<IValidator<UpdateRoleRequestDto>, UpdateRoleRequestValidator>();  // Add this line
        services.AddScoped<IChairmanService, ChairmanService>();
        services.AddScoped<ICaretakerService, CaretakerService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddValidatorsFromAssemblyContaining<CreateChairmanRequestDtoValidator>();
        services.AddScoped<IValidator<UpdateLevyRequestDto>, UpdateLevyRequestDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateLevyRequestDtoValidator>();
        services.AddScoped<IValidator<UpdateProfileDto>, UpdateProfileDtoValidator>();
        services.AddScoped<IValidator<CreateAdminRequestDto>, CreateAdminValidator>();
        services.AddScoped<IValidator<UpdateAdminProfileDto>, UpdateAdminProfileValidator>();
        services.AddScoped<IValidator<CreateRoleRequestDto>, CreateRoleValidator>();
        services.AddScoped<IValidator<UpdateRoleRequestDto>, UpdateRoleValidator>();
        services.AddScoped<IValidator<CreateMarketRequestDto>, CreateMarketRequestValidator>();
        services.AddScoped<IValidator<CaretakerForCreationRequestDto>, CreateCaretakerValidator>();
        services.AddScoped<IValidator<UpdateMarketRequestDto>, UpdateMarketRequestValidator>();
        services.AddScoped<IValidator<CaretakerForCreationRequestDto>, CaretakerForCreationRequestDtoValidator>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IPayments, Payments>();
        services.AddScoped<IPaymentService, PayStackService>();
        services.AddScoped<IGoodBoysService, GoodBoysService>();
        services.AddScoped<IValidator<CreateGoodBoyRequestDto>, CreateGoodBoyRequestValidator>(); // Fix this
        //services.AddScoped<IValidator<UpdateGoodBoyProfileDto>, UpdateGoodBoyProfileValidator>(); // Add this

        // Register repositories
        services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
        services.AddScoped<IBankSettingsService, BankSettingsService>();

        // Register validators
        services.AddScoped<IValidator<CreateAdvertisementRequestDto>, CreateAdvertisementValidator>();
        services.AddScoped<IValidator<UpdateAdvertisementRequestDto>, UpdateAdvertisementValidator>();
        services.AddScoped<IValidator<UploadPaymentProofRequestDto>, UploadPaymentProofValidator>();
        // In your ServiceRegistration.cs
        services.AddScoped<IValidator<CreateAssistantOfficerRequestDto>, CreateAssistantOfficerValidator>();
        services.AddScoped<IValidator<UpdateAssistantOfficerRequestDto>, UpdateAssistantOfficerValidator>();

        // Register services
        services.AddScoped<IAdvertisementService, AdvertisementService>();

        // Add other services
        services.AddScoped<IAdminService, AdminService>();
        services.AddSingleton<ISmsService, AfricasTalkingSmsService>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        services.AddScoped<ICaretakerRepository, CaretakerRepository>();  // Add this line
        services.AddScoped<IMarketRepository, MarketRepository>();        // Add this if needed
        services.AddScoped<IChairmanRepository, ChairmanRepository>();    // Add this if needed
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddScoped<IValidator<CreateAssistantOfficerRequestDto>, CreateAssistantOfficerRequestDtoValidator>();
        services.AddAutoMapper(typeof(MappingProfile));
        // Or if you have multiple profiles:
        services.AddAutoMapper(typeof(Program).Assembly);
        // Current approach - easily replaceable
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpClient<ISmsService, AfricasTalkingSmsService>();

        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromMinutes(10);
        });

        return services;
    }

    public static IServiceCollection AddFirebaseNotifications(this IServiceCollection services)
    {
        services.AddScoped<IFirebaseNotificationService, FirebaseNotificationService>();
        return services;
    }
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            AuthorizationConfiguration.ConfigureAuthorization(options);
        });

        return services;
    }
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Set to true in production
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtSettings:ValidIssuer"],
                ValidAudience = configuration["JwtSettings:ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"])),
                ClockSkew = TimeSpan.Zero
            };

            // Add events for debugging
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError("Authentication failed: {Error}", context.Exception);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Token validated successfully");
                    return Task.CompletedTask;
                }
            };
        });

        services.AddSingleton<IAuthorizationHandler, CaseInsensitiveRoleHandler>();

        return services;
    }
    public static IServiceCollection AddSwaggerWithJWT(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SabiMarket API",
                Version = "v1"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

        return services;
    }
}