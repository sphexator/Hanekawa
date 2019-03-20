﻿using System;
using System.Linq;
using Hanekawa.Entities.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace Hanekawa.Services.Scheduler
{
    public class QuartzJonFactory : IJobFactory, IHanaService
    {
        private readonly IServiceProvider _services;

        public QuartzJonFactory(IServiceProvider services) => _services = services;

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobDetail = bundle.JobDetail;

            var job = (IJob) _services.GetService(jobDetail.JobType);
            return job;
        }

        public void ReturnJob(IJob job)
        {
        }
    }

    public static class UsingQuartz
    {
        public static void UseQuartz(this IServiceCollection services, params Type[] jobs)
        {
            services.AddSingleton<IJobFactory, QuartzJonFactory>();
            services.Add(jobs.Select(jobType => new ServiceDescriptor(jobType, jobType, ServiceLifetime.Singleton)));

            services.AddSingleton(provider =>
            {
                var schedulerFactory = new StdSchedulerFactory();
                var scheduler = schedulerFactory.GetScheduler().Result;
                scheduler.JobFactory = provider.GetService<IJobFactory>();
                scheduler.Start();
                return scheduler;
            });
        }
    }
}