using System;
using System.Linq;
using System.Reflection;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.AnimeSimulCast;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Victoria;

namespace Hanekawa
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var dbClient = new DatabaseClient(Configuration["connectionString"]);
            using (var db = new DbService()) db.Database.Migrate();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton(services);
            services.AddSingleton(Configuration);
            services.AddLogging();
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 35,
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Info
            }));
            services.AddSingleton(new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                LogLevel = LogSeverity.Info,
                DefaultRunMode = RunMode.Async
            }));
            services.AddSingleton<InteractiveService>();
            services.AddSingleton(new AnimeSimulCastClient());
            services.AddSingleton(new LavaSocketClient());
            services.AddSingleton(new LavaRestClient(new Configuration()));
            services.AddSingleton<Random>();
            services.AddHttpClient();

            var assembly = Assembly.GetAssembly(typeof(Program));
            var serviceList = assembly.GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(INService))
                            && !x.GetTypeInfo().IsInterface && !x.GetTypeInfo().IsAbstract).ToList();
            foreach (var x in serviceList)
            {
                services.AddSingleton(x);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

            var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddNLog(new NLogProviderOptions
            {
                CaptureMessageProperties = true,
                CaptureMessageTemplates = true
            });
            try
            {
                LogManager.LoadConfiguration("NLog.config");
            }
            catch
            {
                LogManager.LoadConfiguration("nlog.config");
            }

            await app.ApplicationServices.GetRequiredService<Bot.Hanekawa>().StartAsync();
        }
    }
}