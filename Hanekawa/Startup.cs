using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Hanekawa.Bot.Commands;
using Hanekawa.Database;
using Hanekawa.Entities;
using Hanekawa.HungerGames;
using Hanekawa.HungerGames.Entities.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Hanekawa
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton(Configuration);
            services.AddLogging();
            services.AddPrefixProvider<GuildPrefixProvider>();
            services.AddCommands(e =>
            {
                e.DefaultRunMode = RunMode.Parallel;
                e.StringComparison = StringComparison.OrdinalIgnoreCase;
            });
            services.AddDbContextPool<DbService>(x =>
            {
                x.UseNpgsql(Configuration["connectionString"]);
                x.EnableDetailedErrors();
                x.EnableSensitiveDataLogging(false);
                x.UseLoggerFactory(MyLoggerFactory);
            });
            services.AddSingleton(new HungerGameClient(new Random(), new HungerGameConfig
            {
                LootChance = new LootChanceConfig()
            }));
            services.AddSingleton<Random>();
            services.AddHttpClient();
            services.AddInteractivity();
            var assembly = Assembly.GetEntryAssembly();
            if (assembly is null) return;
            var serviceList = assembly.GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(INService))
                            && !x.GetTypeInfo().IsInterface && !x.GetTypeInfo().IsAbstract).ToList(); 
            foreach (var x in serviceList) 
                services.AddSingleton(x);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            app.UseHttpsRedirection();
            using var scope = app.ApplicationServices.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            db.Database.Migrate();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller}/{action=Index}/{id?}");
            });
        }

        private static readonly ILoggerFactory MyLoggerFactory
            = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter((category, level) =>
                        category == DbLoggerCategory.Update.Name
                        && level == Microsoft.Extensions.Logging.LogLevel.Information)
                    .AddConsole();
            });
    }
}