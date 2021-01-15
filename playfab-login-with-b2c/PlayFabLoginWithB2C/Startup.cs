using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PlayFabLoginWithB2C.Services;
using Microsoft.Identity.Web;
using System.Diagnostics;
using System.Security.Claims;

namespace PlayFabLoginWithB2C
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
            services.AddAuthentication(AzureADB2CDefaults.AuthenticationScheme)
            .AddAzureADB2C(options =>
            {
                Configuration.Bind("AzureAdB2C", options);
            })
            .AddCookie();

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.Configure<PlayFabOptions>(Configuration.GetSection(PlayFabOptions.PlayFab));


            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.HandleSameSiteCookieCompatibility();
            });

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<IPlayFabService, PlayFabService>();

            services.Configure<OpenIdConnectOptions>(AzureADB2CDefaults.OpenIdScheme, options =>
            {
                options.SaveTokens = true;
                options.Events.OnTokenValidated = async context =>
                {
                    var idToken = context.ProtocolMessage.IdToken;
                    using (var sp = services.BuildServiceProvider())
                    {
                        var playfabService = sp.GetService<IPlayFabService>();
                        var response = await playfabService.LoginWithOpenIDConnect(idToken);
                        if (!string.IsNullOrEmpty(response.ErrorMessage))
                        {
                            Debug.Print($"response.ErrorMessage: {response.ErrorMessage}");
                        }
                        else
                        {
                            var identity = ((ClaimsIdentity)context.Principal.Identity);
                            await playfabService.UpdateUserDataWithClaims(identity.Claims);
                            identity.AddClaim(new Claim(PlayFabClaims.PlayFabId, response.AccountInfo.PlayFabId));
                        }
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
