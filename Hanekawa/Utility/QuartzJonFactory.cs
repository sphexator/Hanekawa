using System;
using Hanekawa.Entities;
using Quartz;
using Quartz.Spi;

namespace Hanekawa.Utility
{
    public class QuartzJonFactory : IJobFactory, INService
    {
        private readonly IServiceProvider _services;

        public QuartzJonFactory(IServiceProvider services) => _services = services;

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobDetail = bundle.JobDetail;

            var job = (IJob) _services.GetService(jobDetail.JobType)!;
            return job;
        }

        public void ReturnJob(IJob job)
        {
        }
    }
}