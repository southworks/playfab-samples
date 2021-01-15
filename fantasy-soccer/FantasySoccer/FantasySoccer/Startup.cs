using System.Security.Claims;
using FantasySoccer.Core.Configuration;
using FantasySoccer.Core.Services;
using FantasySoccer.Models.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace FantasySoccer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"));

            services.AddControllersWithViews();

            services.AddRazorPages()
                .AddMicrosoftIdentityUI();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<Services.IAuthenticationService, Services.AuthenticationService>();

            services.Configure<PlayFabConfiguration>(Configuration.GetSection(PlayFabConfiguration.PlayFab));
            services.AddSingleton(serviceProvider => new PlayFabConfiguration()
            {
                TitleId = Configuration.GetSection(PlayFabConfiguration.PlayFab).GetSection("TitleId").Value,
                ConnectionId = Configuration.GetSection(PlayFabConfiguration.PlayFab).GetSection("ConnectionId").Value,
                DeveloperSecretKey = Configuration.GetSection(PlayFabConfiguration.PlayFab).GetSection("DeveloperSecretKey").Value,
                CatalogName = Configuration.GetSection(PlayFabConfiguration.PlayFab).GetSection("CatalogName").Value,
                StoreName = Configuration.GetSection(PlayFabConfiguration.PlayFab).GetSection("StoreName").Value,
                Currency = Configuration.GetSection(PlayFabConfiguration.PlayFab).GetSection("Currency").Value,
                AllUserSegmentId = Configuration.GetSection(PlayFabConfiguration.PlayFab).GetSection("AllUserSegmentId").Value
            });
            services.AddSingleton(serviceProvider => new CosmosDBConfig()
            {
                PrimaryKey = Configuration.GetSection(CosmosDBConfig.CosmosDB).GetSection("PrimaryKey").Value,
                EndpointUri = Configuration.GetSection(CosmosDBConfig.CosmosDB).GetSection("EndpointUri").Value,
                DatabaseName = Configuration.GetSection(CosmosDBConfig.CosmosDB).GetSection("DatabaseName").Value
            });
            services.AddSingleton<ICosmosDBService, CosmosDBService>();
            services.AddSingleton<IPlayFabService, PlayFabService>();
            services.AddSingleton<IFantasySoccerService, FantasySoccerService>();
            services.AddSingleton<ISimulationService, SimulationService>();

            services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.SaveTokens = true;

                options.Events.OnTicketReceived = async context =>
                {
                    var identity = (ClaimsIdentity)context.Principal.Identity;
                    var authenticateResult = await context.HttpContext.AuthenticateAsync();
                    var fsAuthService = context.HttpContext.RequestServices.GetService<Services.IAuthenticationService>();
                    var idToken = context.Properties.GetTokenValue("id_token");
                    var response = await fsAuthService.LoginWithOpenIDConnectAsync(idToken, identity, context.Properties);

                    if (string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        identity.AddClaim(new Claim(Helpers.Constants.Authentication.SchemaClaim, Helpers.Constants.Authentication.MicrosoftSchema));
                    }
                    else
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Fail($"OpenId failed to do login into PlayFab. {response.ErrorMessage}");
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
