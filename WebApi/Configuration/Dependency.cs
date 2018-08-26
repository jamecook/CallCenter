using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Services;

namespace WebApi.Configuration
{
    public static class Dependency
    {
        public static IServiceCollection AddServiceDependencies(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            //services.AddSingleton<IAuthorizationHandler, MaximumOfficeNumberAuthorizationHandler>();  
            return services;
        }
    }
}
