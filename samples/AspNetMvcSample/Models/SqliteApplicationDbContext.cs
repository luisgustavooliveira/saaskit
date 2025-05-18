using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace AspNetMvcSample.Models
{
    // https://docs.microsoft.com/en-us/ef/
    public class SqliteApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IWebHostEnvironment env;
        private readonly AppTenant tenant;

        public SqliteApplicationDbContext(IWebHostEnvironment env, AppTenant tenant)
        {
            this.env = env;
            this.tenant = tenant;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var tenantDbName = tenant.Name.Replace(" ", "-").ToLowerInvariant();
            var connectionString = $"FileName={tenantDbName}.db";
            optionsBuilder.UseSqlite(connectionString);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
