using AspNetCore.Scheduler.Quartz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Scheduler.ServiceTemplate;

namespace NonWebAppTemplate
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch
            {
                return 1;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                // Configuration paths
                .ConfigureServices(services =>
                {
                    var configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build();
                    services.AddQuartz(configuration.GetSection("Quartz"));
                })

                // configuration
                .ConfigureServices(services => { services.RegisterJob<HelloWorldJob>(); });
    }
}