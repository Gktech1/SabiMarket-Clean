using Microsoft.EntityFrameworkCore;
using SabiMarket.API.Extensions;
using SabiMarket.API.Middlewares;
using SabiMarket.Infrastructure.Data;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddControllers();
        builder.Services.AddDatabaseContext(builder.Configuration);
        builder.Services.AddCustomErrorHandling();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddCorsPolicy();
        builder.Services.AddScoped<RequestTimeLoggingMiddleware>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerWithJWT();
        builder.Services.AddCustomAuthorization();
        builder.Services.AddFirebaseNotifications();

        var app = builder.Build();

        // Database Migration and Seeding
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();
                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                await seeder.SeedAsync();
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while initializing the database.");
            }
        }

        // Configure middleware
        //if (app.Environment.IsDevelopment())
        //{
            app.UseSwagger();
            app.UseSwaggerUI();
        //}

        app.UseMiddleware<RequestTimeLoggingMiddleware>();
        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseCustomErrorHandling();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        await app.RunAsync();
    }
}