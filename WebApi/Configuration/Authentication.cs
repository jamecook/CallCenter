using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace WebApi.Configuration
{
    public static class Authentication
    {
        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o => Configure(o, configuration));
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("OfficeNumberUnder200", policy => policy.Requirements.Add(new MaximumOfficeNumberRequirement(200)));
            //});
            return services;
        }

        public static MvcOptions AddGlobalAuthControl(this MvcOptions options)
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
            return options;
        }

        private static void Configure(JwtBearerOptions options, IConfiguration configuration)
        {
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                //ValidateIssuer = true,
                ValidIssuer = configuration["Auth:Issuer"],
                ValidateAudience = false,
                //ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(configuration["Auth:Key"])),
                //ValidateIssuerSigningKey = true,

            };
            options.Events = new JwtBearerEvents()
            {
                OnAuthenticationFailed = OnAuthenticationFailed,
                OnTokenValidated = OnTokenValidated,
                OnChallenge = OnChallenge,
                OnMessageReceived = OnMessageReceived
            };
        }

        private static Task OnMessageReceived(MessageReceivedContext messageReceivedContext)
        {
            return Task.FromResult(0);
        }

        private static Task OnChallenge(JwtBearerChallengeContext jwtBearerChallengeContext)
        {
            return Task.FromResult(0);
        }

        private static Task OnTokenValidated(TokenValidatedContext arg)
        {
            //var s = $"Authentication Success: {arg.SecurityToken}";
            //arg.Response.ContentLength = s.Length;
            //arg.Response.Body.Write(Encoding.UTF8.GetBytes(s), 0, s.Length);
            return Task.FromResult(0);
        }

        private static Task OnAuthenticationFailed(AuthenticationFailedContext arg)
        {
            var s = $"AuthenticationFailed: {arg.Exception.Message}";
            arg.Response.ContentLength = s.Length;
            arg.Response.Body.Write(Encoding.UTF8.GetBytes(s), 0, s.Length);
            return Task.FromResult(0);
        }
    }


    internal class MaximumOfficeNumberAuthorizationHandler : AuthorizationHandler<MaximumOfficeNumberRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MaximumOfficeNumberRequirement requirement)
        {
            // Bail out if the office number claim isn't present
            if (!context.User.HasClaim(c => c.Issuer == "http://localhost:5000/" && c.Type == "office"))
            {
                return Task.CompletedTask;
            }

            // Bail out if we can't read an int from the 'office' claim
            int officeNumber;
            if (!int.TryParse(context.User.FindFirst(c => c.Issuer == "http://localhost:5000/" && c.Type == "office").Value, out officeNumber))
            {
                return Task.CompletedTask;
            }

            // Finally, validate that the office number from the claim is not greater
            // than the requirement's maximum
            if (officeNumber <= requirement.MaximumOfficeNumber)
            {
                // Mark the requirement as satisfied
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    // A custom authorization requirement which requires office number to be below a certain value
    internal class MaximumOfficeNumberRequirement : IAuthorizationRequirement
    {
        public MaximumOfficeNumberRequirement(int officeNumber)
        {
            MaximumOfficeNumber = officeNumber;
        }

        public int MaximumOfficeNumber { get; private set; }
    }
}
