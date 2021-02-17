using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetCore.Scheduler.Quartz.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace AspNetCore.Scheduler.Quartz
{
    public static class ServiceExtensions
    {
        private static Configurations.Quartz _quartzConfig;

        public static void AddQuartz(this IServiceCollection services, IConfigurationSection quartzConfigSection)
        {
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddHostedService<QuartzHostedService>();
            services.Configure<Configurations.Quartz>(quartzConfigSection);
            _quartzConfig = quartzConfigSection.Get<Configurations.Quartz>();
        }

        private static void AddJobSchedule<T>(this IServiceCollection services)
        {
            var tType = typeof(T);
            var jobName = tType.FullName;
            var types = tType.Assembly.GetTypes();
            var typeFullNames = types.Select(i => i.FullName).Where(i => i == jobName);
            //var keys = _quartzConfig.Jobs.Keys.Intersect(typeFullNames);
            var triggers = _quartzConfig.Triggers.Where(t => t.Value.Select(j => j.ClassFullName).Intersect(typeFullNames).Any());

            Console.WriteLine("Jobs below are registered:");
            foreach (var trigger in triggers)
            {
                Console.WriteLine($"\t{trigger}");
                foreach (var job in trigger.Value)
                {
                    Console.WriteLine($"\t{job}");
                    var type = types.Single(t => t.FullName == job.ClassFullName);
                    services.AddSingleton(new JobSchedule(
                        jobType: type,
                        cronExpression: trigger.Key));
                }
            }
        }

        [Obsolete("This function is deprecated, please use AddSingletonJob instead.", true)]
        public static void RegisterJob<T>(this IServiceCollection services) where T : class, IJob
        {
            services.AddSingletonJob<T>();
        }
        // TODO: research
        //public static void AddScopedJob<T>(this IServiceCollection services) where T : class, IJob
        //{
        //    services.AddJobSchedule<T>();
        //    services.AddScoped<T>();
        //}
        public static void AddTransientJob<T>(this IServiceCollection services) where T : class, IJob
        {
            services.AddJobSchedule<T>();
            services.AddTransient<T>();
        }
        public static void AddSingletonJob<T>(this IServiceCollection services) where T : class, IJob
        {
            services.AddJobSchedule<T>();
            services.AddSingleton<T>();
        }
        public static void AddExtraneousTransientJobs(this IServiceCollection services)
        {
            foreach (var trigger in _quartzConfig.Triggers)
            {
                foreach (var job in trigger.Value)
                {
                    var jobSerialized = JsonSerializer.Serialize(job);
                    Console.WriteLine("Adding the job:");
                    Console.WriteLine(jobSerialized);
                    var assembly = TryLoadAssembly(job);
                    //var type = Type.GetType(job.ClassFullName);
                    var type = assembly.GetType(job.ClassFullName);
                    if (type == null)
                    {
                        Console.WriteLine("Could not register!");
                        Console.WriteLine(job.ClassFullName);
                        Console.WriteLine();
                        continue;
                    }
                    services.AddSingleton(new JobSchedule(
                        jobType: type,
                        cronExpression: trigger.Key));
                    services.AddTransient(type);

                }
            }
        }

        private static Assembly TryLoadAssembly(Job job)
        {
            try
            {
                if (string.IsNullOrEmpty(job.AssemblyFilename)) throw new Exception("Could not find AssemblyName.");

                var ServiceFolder = Path.IsPathRooted(job.AssemblyDirectory) ? job.AssemblyDirectory : $"{AppDomain.CurrentDomain.BaseDirectory}{job.AssemblyDirectory}";
                var assemblyFileName = $"{ServiceFolder}{job.AssemblyFilename}";

                Console.WriteLine("Loading the assembly:");
                Console.WriteLine(assemblyFileName);
                var assembly = Assembly.LoadFrom(assemblyFileName);
                Console.WriteLine("Success!");
                return assembly;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failure: Could not load the assembly.");
                Console.WriteLine(e);
                return null;
            }
        }

        public static IHost RunRegisteredJobOnStartAsync(this IHost host)
        {
            Console.WriteLine("Started running registered jobs on start!");
            var types = new List<Type>();
            var ti = typeof(IJob);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in asm.GetTypes())
                {
                    if (ti.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                    {
                        types.Add(t);
                    }
                }
            }
            var serviceTypes = host.Services.GetServices<JobSchedule>().Select(s => s.JobType).Where(s => types.Contains(s));
            var tasks = new List<Task>();
            foreach (var serviceType in serviceTypes)
            {
                var services = host.Services.GetServices(serviceType);
                foreach (var service in services)
                {
                    var job = (IJob)service;
                    tasks.Add(job.Execute(null));
                }
            }
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("End running registered jobs on start!");
            return host;
        }
    }
}