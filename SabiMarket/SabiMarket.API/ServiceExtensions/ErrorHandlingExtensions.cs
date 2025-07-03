using SabiMarket.API.Middlewares;

namespace SabiMarket.API.Extensions
{
    public static class ErrorHandlingExtensions
    {
        public static IServiceCollection AddCustomErrorHandling(this IServiceCollection services)
        {
            services.AddTransient<ErrorHandlingMiddleware>();
            return services;
        }

        public static IApplicationBuilder UseCustomErrorHandling(this IApplicationBuilder app)
        {
            app.UseMiddleware<ErrorHandlingMiddleware>();
            return app;
        }
    }
}