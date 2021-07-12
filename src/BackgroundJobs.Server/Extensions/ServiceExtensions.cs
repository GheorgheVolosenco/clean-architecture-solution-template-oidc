using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Presentation.Common;

namespace BackgroundJobs.Server.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddAuthenticationViaOIDC(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();
            services.AddAuthentication(config =>
            {
                config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
               .AddCookie()
               .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, config =>
               {
                   config.Authority = configuration["OidcProvider:Authority"];
                   config.ClientId = configuration["OidcProvider:ClientId"];
                   config.ClientSecret = configuration["OidcProvider:ClientSecret"];
                   config.SaveTokens = true;
                   config.ResponseType = OpenIdConnectResponseType.Code;
                   config.Scope.Add("roles");
                   config.GetClaimsFromUserInfoEndpoint = true;
                   config.UsePkce = true;
               });
        }
    }
}
