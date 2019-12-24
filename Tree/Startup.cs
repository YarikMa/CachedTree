namespace Tree
{
    using Data;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Models;

    using Services;

    public class Startup
    {
        #region Properties

        public IConfiguration Configuration { get; }

        #endregion

        #region Constructors

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        #region Methods

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddControllers();
            
            services.AddSingleton<DbTreeContext>(provider =>
            {
                var dbContext = new DbTreeContext();
                // just for seeding on first creation
                dbContext.Seed();
                return dbContext;
            });
            services.AddSingleton<CachedTreeContext>();
            services.AddTransient<IDbTreeRepository, DbTreeRepository>();
            services.AddTransient<ICachedTreeRepository, CachedTreeRepository>();
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
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapRazorPages();
                    endpoints.MapControllers();
                });
        }

        #endregion
    }
}
