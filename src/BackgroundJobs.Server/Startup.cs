using Application;
using Application.Constants;
using BackgroundJobs.Server.Extensions;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Persistence;
using Infrastructure.Shared;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BackgroundJobs.Server
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var databaseConnectionString = this.Configuration.GetConnectionString("DefaultConnection");

            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(databaseConnectionString));

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthenticationViaOIDC(this.Configuration);

            services.AddAuthorization(cfg =>
            {
                cfg.AddPolicy("Hangfire", cfgPolicy =>
                {
                    cfgPolicy.AddRequirements().RequireAuthenticatedUser();
                    cfgPolicy.AddAuthenticationSchemes(OpenIdConnectDefaults.AuthenticationScheme);
                    cfgPolicy.RequireRole(Roles.HangfireAccess);
                });
            });

            services.AddApplicationLayer();
            services.AddPersistenceInfrastructure(this.Configuration);
            services.AddSharedInfrastructure(this.Configuration);
            services.AddHealthChecks();
            services.AddHangfireServer(options => options.WorkerCount = 2);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseHealthChecks("/health");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHangfireDashboard().RequireAuthorization("Hangfire");
            });
        }
    }
}
