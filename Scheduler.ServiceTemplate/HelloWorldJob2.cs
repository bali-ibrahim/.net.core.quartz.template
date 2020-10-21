using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Scheduler.ServiceTemplate
{
    [DisallowConcurrentExecution]
    public class HelloWorldJob2 : IJob
    {
        private readonly ILogger<HelloWorldJob2> _logger;

        public HelloWorldJob2(ILogger<HelloWorldJob2> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            const string text = "Hello World!";
            _logger.LogInformation(text);
            Console.WriteLine(text);
            return Task.CompletedTask;
        }
    }
}