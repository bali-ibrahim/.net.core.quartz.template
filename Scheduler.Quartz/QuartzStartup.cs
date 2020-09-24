using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace Scheduler.Quartz
{
    public class QuartzStartup
    {
        public QuartzStartup(IConfiguration configuration
            //, ILogger<QuartzStartup> logger
        )
        {
            Configuration = configuration;
            //_logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddHostedService<QuartzHostedService>();

            //services.AddSingleton<HelloWorldJob>();


            ConfigureQuartz<Quartz>(services);
        }

        public void ConfigureQuartz<T>(IServiceCollection services) where T : Quartz
        {
            var appSettingsSection = Configuration.GetSection(typeof(T).Name);
            services.Configure<T>(appSettingsSection);
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
                services.AddSingleton(new JobSchedule(
                    jobType: type,
                    cronExpression: appSettings.Jobs[key])); //every 10 seconds
            }
        }
    }
}