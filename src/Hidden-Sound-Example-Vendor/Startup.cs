using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace VendorTestProject
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("Local"));
            });

            services.AddAuthentication(options => {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext, int>()
                .AddDefaultTokenProviders();

            services.AddSingleton<HttpClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseIdentity();

            // Insert a new cookies middleware in the pipeline to store the user
            // identity after he has been redirected from the identity provider.
            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    AutomaticAuthenticate = false,
            //    AutomaticChallenge = false,
            //    LoginPath = new PathString("/signin")
            //});

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                // Note: these settings must match the application details
                // inserted in the database at the server level.
                ClientId = "mvc",
                ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                PostLogoutRedirectUri = "http://localhost:52191/",

                RequireHttpsMetadata = false,
                GetClaimsFromUserInfoEndpoint = true,
                SaveTokens = true,
                Events = new OpenIdConnectEvents()
                {
                  OnTicketReceived = async ctx =>
                  {
                      var service = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                      var user1 = await service.GetUserAsync(ctx.HttpContext.User);
                      var user2 = await service.GetUserAsync(ctx.Principal);

                      await Task.FromResult(0);
                  },
                  OnTokenResponseReceived = async ctx =>
                  {
                      // ctx.SkipToNextMiddleware();
                      var service = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                      var user1 = await service.GetUserAsync(ctx.HttpContext.User);

                      await Task.FromResult(0);
                  },
                  OnRedirectToIdentityProvider = ctx =>
                  {
                      ctx.SkipToNextMiddleware();
                      ctx.HttpContext.Items.Add("CurrentUser", ctx.HttpContext.User);
                      return Task.FromResult(0);
                  }
                  ,
                    OnMessageReceived = ctx =>
                    {
                        return Task.FromResult(0);
                    },
                    OnAuthorizationCodeReceived = ctx =>
                    {
                        return Task.FromResult(0);
                    }
                },

                // Use the authorization code flow.
                ResponseType = OpenIdConnectResponseType.Code,
                AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet,

                // Note: setting the Authority allows the OIDC client middleware to automatically
                // retrieve the identity provider's configuration and spare you from setting
                // the different endpoints URIs or the token validation parameters explicitly.
                Authority = "http://localhost:60584/",

                Scope = { "email", "roles" }
            });

            app.UseMvcWithDefaultRoute();
        }
    }
}
