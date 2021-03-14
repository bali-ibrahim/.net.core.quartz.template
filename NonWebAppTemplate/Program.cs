using System.Threading.Tasks;
using AspNetCore.Scheduler.Quartz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace NonWebAppTemplate
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
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
                .ConfigureServices(services =>
                {
                    services.AddTransientJob<HelloWorldJob>(true);
                    services.AddTransientJob<HelloWorldJob2>(true);
                })
                ;
    }
}