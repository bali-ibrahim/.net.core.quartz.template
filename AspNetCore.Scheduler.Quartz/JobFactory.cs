using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;

namespace AspNetCore.Scheduler.Quartz
{
    public class JobFactory : IJobFactory
    {
        private readonly ILogger logger;
        private readonly IServiceProvider _serviceProvider;

        public JobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            // https://stackoverflow.com/a/32315573/7032856
            try
            {
                return _serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception creating job. Giving up and returning a do-nothing logging job.");
                var job = new DummyJob();
                return job;
            }
        }

        public void ReturnJob(IJob job)
        {
            // we let the DI container handler this
        }
    }
}