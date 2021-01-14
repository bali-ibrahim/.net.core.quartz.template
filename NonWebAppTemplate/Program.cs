using AspNetCore.Scheduler.Quartz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace NonWebAppTemplate
{
    public class Program
    {
        public static int Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            return 0;
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
                .ConfigureServices(services => { services.AddExtraneousTransientJobs(); });
    }
}