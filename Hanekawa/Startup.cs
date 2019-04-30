using System;
using System.Linq;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                MessageCacheSize = 35,
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Info
            }));
            services.AddSingleton(new CommandService(new CommandServiceConfiguration
            {
                CaseSensitive = false,
                DefaultRunMode = RunMode.Parallel
            }));
            
            services.AddDbContextPool<DbService>(x =>
            {
                x.UseNpgsql("");
                x.EnableDetailedErrors();
                x.EnableSensitiveDataLogging();
            }, 200);
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            _ = app.ApplicationServices.GetRequiredService<Bot.Hanekawa>().StartAsync();
        }
    }
}