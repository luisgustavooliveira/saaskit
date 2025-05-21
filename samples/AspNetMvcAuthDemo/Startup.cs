using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace AspNetMvcAuthSample
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            builder.AddUserSecrets<Startup>();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(builder =>
			{
				builder.AddConsole();
			});

			services.AddMultitenancy<AppTenant, CachingAppTenantResolver>();

            // Add framework services.
            services.AddMvc();

            services.Configure<MultitenancyOptions>(Configuration.GetSection("Multitenancy"));

		}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseDeveloperExceptionPage();
			
            app.UseStaticFiles();

            app.UseMultitenancy<AppTenant>();

			app.UsePerTenant<AppTenant>((ctx, builder) =>
			{
				builder.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
		.       AddCookie(options =>
				{
                    options.LoginPath = new PathString("/account/login");
                    options.AccessDeniedPath = new PathString("/account/forbidden");
				});

				// only register for google if ClientId and ClientSecret both exist
				var clientId = Configuration[$"{ctx.Tenant.Id}:GoogleClientId"];
				var clientSecret = Configuration[$"{ctx.Tenant.Id}:GoogleClientSecret"];

				if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
				{
					builder.UseGoogleAuthentication(new GoogleOptions()
					{
						AuthenticationScheme = "Google",
						SignInScheme = "Cookies",

						ClientId = clientId,
						ClientSecret = clientSecret
					});
				}
			});

			app.UseMvc(routes =>
            {
                routes.MapRoute(
					name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
