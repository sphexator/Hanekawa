using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Hanekawa.AnimeSimulCast;
using Hanekawa.Bot.Services.Administration.Warning;
using Hanekawa.Database;
using Hanekawa.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interactive;
using Hanekawa.Shared.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Qmmands;
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
            using (var db = new DbService())
            {
                db.Database.Migrate();
            }

            services.AddControllers();
            services.AddHostedService<Bot.Hanekawa>();
            services.AddSingleton(services);
            services.AddSingleton(Configuration);
            services.AddLogging();
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 35,
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Info
            }));
            services.AddSingleton(new CommandService(new CommandServiceConfiguration
            {
                DefaultRunMode = RunMode.Parallel,
                CooldownBucketKeyGenerator = (obj, cxt, provider) =>
                {
                    var context = (HanekawaContext) cxt;
                    return context.User.Id;
                }
            }));
            services.AddSingleton<InteractiveService>();
            services.AddSingleton(new AnimeSimulCastClient());
            services.AddSingleton(new LavaSocketClient());
            services.AddSingleton(new LavaRestClient(new Configuration
            {
                AutoDisconnect = true,
                SelfDeaf = false,
                LogSeverity = LogSeverity.Info,
                PreservePlayers = true
            }));
            services.AddSingleton<Random>();
            services.AddSingleton<HttpClient>();
            services.UseQuartz(typeof(WarnService));

            var assembly = Assembly.GetAssembly(typeof(Program));
            var serviceList = assembly.GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(INService))
                            && !x.GetTypeInfo().IsInterface && !x.GetTypeInfo().IsAbstract).ToList();
            for (var i = 0; i < serviceList.Count; i++)
            {
                var x = serviceList[i];
                if (x.GetInterfaces().Contains(typeof(INService))) services.AddSingleton(x);
                else if (x.GetInterfaces().Contains(typeof(INService))) services.AddTransient(x);
                else if (x.GetInterfaces().Contains(typeof(INService))) services.AddScoped(x);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

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
        }
    }
}