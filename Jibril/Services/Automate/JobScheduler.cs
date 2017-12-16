using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Jibril.Services.Automate.PicDump;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;

namespace Jibril.Services.Automate
{
    public class JobScheduler
    {
        private IServiceProvider _service;
        public JobScheduler(IServiceProvider service)
        {
            this._service = service;
            SchedulerTask().ConfigureAwait(false);
        }
        public static async Task SchedulerTask()
        {
            try
            {
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                var factory = new StdSchedulerFactory(props);
                var scheduler = await factory.GetScheduler();

                await scheduler.Start();

                var job = JobBuilder.Create<PostPictures>()
                    .WithIdentity("job1", "group1")
                    .Build();
                
                var trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    //.StartNow()
                    .WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(18, 15, DayOfWeek.Saturday))
                    //.WithCronSchedule("0 5 18 ? * SAT")
                    .Build();

                await scheduler.ScheduleJob(job, trigger);
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
        }

        private class ConsoleLogProvider : ILogProvider
        {
            public Logger GetLogger(string name)
            {
                return (level, func, exception, parameters) =>
                {
                    if (level >= LogLevel.Info && func != null)
                    {
                        Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
                    }
                    return true;
                };
            }

            public IDisposable OpenNestedContext(string message)
            {
                throw new NotImplementedException();
            }

            public IDisposable OpenMappedContext(string key, string value)
            {
                throw new NotImplementedException();
            }
        }
    }

}