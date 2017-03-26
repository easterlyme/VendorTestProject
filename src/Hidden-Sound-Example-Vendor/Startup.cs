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

            services.AddCors(options =>
            {
                options.AddPolicy("HiddenSound", p => p.WithOrigins("https://localhost:44333").AllowAnyMethod().AllowAnyHeader());
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

            // app.UseCors("HiddenSound");

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                // Note: these settings must match the application details
                // inserted in the database at the server level.
                ClientId = "t4JTk7f8K3NmGv0q9X5Scs6I2",
                ClientSecret = "y4YHo7s6A8ChBp01Oue2MKr93Jdk5X6WqPt8n3I2FaDj15Rfw9",
                PostLogoutRedirectUri = "http://localhost:52191/",

                RequireHttpsMetadata = false,
                GetClaimsFromUserInfoEndpoint = true,
                SaveTokens = true,

                // Use the authorization code flow.
                ResponseType = OpenIdConnectResponseType.Code,
                AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet,

                // Note: setting the Authority allows the OIDC client middleware to automatically
                // retrieve the identity provider's configuration and spare you from setting
                // the different endpoints URIs or the token validation parameters explicitly.
                Authority = "https://localhost:44333/",

                CallbackPath = new PathString("/Cart/Test"),
                Events = new OpenIdConnectEvents
                {
                    OnAuthorizationCodeReceived = context =>
                    {

                        return Task.FromResult(0);
                    },
                    OnRedirectToIdentityProvider = context =>
                    {

                        return Task.FromResult(0);
                    }
                }
            });

            app.UseMvcWithDefaultRoute();
        }
    }
}
