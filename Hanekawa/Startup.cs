using System;
using System.Linq;
using System.Reflection;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Hanekawa.Bot.Commands;
using Hanekawa.Bot.Service.Administration.Warning;
using Hanekawa.Bot.Service.Boost;
using Hanekawa.Bot.Service.Experience;
using Hanekawa.Bot.Service.Game;
using Hanekawa.Bot.Service.Mvp;
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
using Npgsql;
using Qmmands;
using Quartz;

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
            services.AddInteractivityExtension();
            var assembly = Assembly.GetEntryAssembly();
            if (assembly is null) return;
            var serviceList = assembly.GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(INService))
                            && !x.GetTypeInfo().IsInterface).ToList();
            foreach (var x in serviceList)
                services.AddSingleton(x);
            
            services.Configure<QuartzOptions>(e =>
            {
                e.Scheduling.IgnoreDuplicates = false;
                e.Scheduling.OverWriteExistingData = true;
            });
            services.AddQuartz(e =>
            {
                e.SchedulerId = "Hanekawa-Scheduler";
                e.UseJobAutoInterrupt(x => x.DefaultMaxRunTime = TimeSpan.FromMinutes(10));
                e.ScheduleJob<ExpService>(x =>
                {
                    x.WithIdentity("Decay");
                    x.WithDescription("Experience decay ran hourly");
                    x.WithCronSchedule("0 0 0/1 1/1 * ? *");
                });
                e.ScheduleJob<WarnService>(x =>
                {
                    x.WithIdentity("WarnDecay");
                    x.WithDescription("Sets old warnings as invalid after X time");
                    x.WithCronSchedule("0 0 13 1/1 * ? *");
                });
                e.ScheduleJob<BoostService>(x =>
                {
                    x.WithIdentity("Boost");
                    x.WithDescription("Weekly reward server boosters");
                    x.WithCronSchedule("0 0 12 ? * MON *");
                });
                e.ScheduleJob<HungerGameService>(x =>
                {
                    x.WithIdentity("HungerGame");
                    x.WithDescription("Handler for rounds for hunger games");
                    x.WithCronSchedule("0 0 0/3 1/1 * ? *");
                });
                e.ScheduleJob<MvpService>(x =>
                {
                    x.WithIdentity("MVP");
                    x.WithDescription("Weekly reward server MVPs / most active");
                    x.WithCronSchedule("0 0 18 1/1 * ? *");
                });
            });
            services.AddQuartzHostedService(x => x.WaitForJobsToComplete = true);
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            app.UseHttpsRedirection();
            NpgsqlConnection.GlobalTypeMapper.UseJsonNet();
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
                        && level == LogLevel.Information)
                    .AddConsole();
            });
    }
}