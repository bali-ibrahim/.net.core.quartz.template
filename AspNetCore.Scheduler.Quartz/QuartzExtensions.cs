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
        private static bool _isConfigured;

        private static void Configure(this IServiceCollection services)
        {
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddHostedService<QuartzHostedService>();
            _isConfigured = true;
        }

        private static void LookForJobsIn(this IConfiguration configuration, IServiceCollection services, Assembly asm)
        {
            var appSettingsSection = configuration.GetSection(typeof(Quartz).Name);
            services.Configure<Quartz>(appSettingsSection);
            var appSettings = appSettingsSection.Get<Quartz>();
            var types = asm.GetTypes();
            var typeFullNames = types.Select(i => i.FullName);
            var keys = appSettings.Jobs.Keys.Intersect(typeFullNames);

            Console.WriteLine("Jobs below are registered:");
            foreach (var key in keys)
            {
                Console.WriteLine($"\t{key}");
                var type = types.Single(t => t.FullName == key);
                services.AddSingleton(new JobSchedule(
                    jobType: type,
                    cronExpression: appSettings.Jobs[key])); //every 10 seconds
            }
        }

        public static void RegisterJob<T>(this IConfiguration configuration, IServiceCollection services) where T : IJob
        {
            if (!_isConfigured) services.Configure();
            var type = typeof(T);
            configuration.LookForJobsIn(services, type.Assembly);
            services.AddSingleton(type);
        }
    }
}