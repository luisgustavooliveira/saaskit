using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AspNetMvcSample.Models;
using AspNetMvcSample.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace AspNetMvcSample
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
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
			services
                .AddEntityFrameworkSqlite()
                .AddDbContext<SqliteApplicationDbContext>();

			services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<SqliteApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddOptions();
            services.AddMvc();
             
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new TenantViewLocationExpander());
            });

            services.Configure<MultitenancyOptions>(Configuration.GetSection("Multitenancy"));

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
			
            app.UseStaticFiles();
            app.UseMultitenancy<AppTenant>();

            // To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseRouting();
            app.UseAuthorization();

			app.UseEndpoints(configure =>
			{
				configure.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
    }
}
