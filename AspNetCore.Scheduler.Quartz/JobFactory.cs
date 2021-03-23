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
            IJob job;
            // https://stackoverflow.com/a/32315573/7032856
            try
            {
                job = _serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            }
            catch (Exception ex)
            {
                const string message = "Exception creating job. Giving up and returning a do-nothing logging job.";
                try
                {
                    logger?.LogError(ex, message);
                }
                catch
                {
                    Console.WriteLine(message);
                }
            }
            finally
            {
                job = new DummyJob();
            }
            return job;
        }

        public void ReturnJob(IJob job)
        {
            // we let the DI container handler this
        }
    }
}