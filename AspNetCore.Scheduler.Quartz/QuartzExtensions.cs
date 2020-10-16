using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace AspNetCore.Scheduler.Quartz
{
    public static class QuartzExtensions
    {
        private static QuartzConfig _quartzConfig;

        public static void AddQuartz(this IServiceCollection services, IConfigurationSection quartzConfigSection)
        {
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddHostedService<QuartzHostedService>();
            services.Configure<QuartzConfig>(quartzConfigSection);
            _quartzConfig = quartzConfigSection.Get<QuartzConfig>();
        }

        private static void LookForJobsIn(this IServiceCollection services, Assembly asm)
        {
            var types = asm.GetTypes();
            var typeFullNames = types.Select(i => i.FullName);
            var keys = _quartzConfig.Jobs.Keys.Intersect(typeFullNames);

            Console.WriteLine("Jobs below are registered:");
            foreach (var key in keys)
            {
                Console.WriteLine($"\t{key}");
                var type = types.Single(t => t.FullName == key);
                services.AddSingleton(new JobSchedule(
                    jobType: type,
                    cronExpression: _quartzConfig.Jobs[key]));
            }
        }

        public static void RegisterJob<T>(this IServiceCollection services) where T : class, IJob
        {
            services.LookForJobsIn(typeof(T).Assembly);
            services.AddSingleton<T>();
        }
    }
}