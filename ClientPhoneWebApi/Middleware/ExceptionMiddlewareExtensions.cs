using Microsoft.AspNetCore.Builder;

namespace ClientPhoneWebApi
{
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandler(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Middleware.ExceptionMiddleware>();
        }
    }
}
