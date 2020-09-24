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
    public class QuartzStartup
    {
        public QuartzStartup(IConfiguration configuration, IServiceCollection services
            //, ILogger<QuartzStartup> logger
        )
        {
            Configuration = configuration;
            Services = services;
            //_logger = logger;
        }

        public IConfiguration Configuration { get; }
        public IServiceCollection Services { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices()
        {
            Services.AddSingleton<IJobFactory, JobFactory>();
            Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            Services.AddHostedService<QuartzHostedService>();

            //services.AddSingleton<HelloWorldJob>();


            //ConfigureQuartz<Quartz>(services);
        }

        private void ConfigureQuartz<T>() where T : Quartz
        {
            var appSettingsSection = Configuration.GetSection(typeof(T).Name);
            Services.Configure<T>(appSettingsSection);
            var appSettings = appSettingsSection.Get<T>();
            var asm = typeof(T).Assembly;
            var types = asm.GetTypes();
            var typeFullNames = types.Select(i => i.FullName);
            var keys = appSettings.Jobs.Keys.Intersect(typeFullNames);

            Console.WriteLine("Jobs below are registered:");
            foreach (var key in keys)
            {
                Console.WriteLine($"\t{key}");
                var type = types.Single(t => t.FullName == key);
                Services.AddSingleton(new JobSchedule(
                    jobType: type,
                    cronExpression: appSettings.Jobs[key])); //every 10 seconds
            }
        }

        private void LookForJobsIn(Assembly asm)
        {
            var appSettingsSection = Configuration.GetSection(typeof(Quartz).Name);
            Services.Configure<Quartz>(appSettingsSection);
            var appSettings = appSettingsSection.Get<Quartz>();
            var types = asm.GetTypes();
            var typeFullNames = types.Select(i => i.FullName);
            var keys = appSettings.Jobs.Keys.Intersect(typeFullNames);

            Console.WriteLine("Jobs below are registered:");
            foreach (var key in keys)
            {
                Console.WriteLine($"\t{key}");
                var type = types.Single(t => t.FullName == key);
                Services.AddSingleton(new JobSchedule(
                    jobType: type,
                    cronExpression: appSettings.Jobs[key])); //every 10 seconds
            }
        }

        public void RegisterJob<T>() where T : IJob
        {
            var type = typeof(T);
            LookForJobsIn(type.Assembly);
            Services.AddSingleton(type);
        }
    }
}