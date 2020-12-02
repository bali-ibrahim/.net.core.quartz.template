using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Scheduler.ServiceTemplate
{
    [DisallowConcurrentExecution]
    public class HelloWorldJob : IJob
    {
        private readonly ILogger<HelloWorldJob> _logger;

        private readonly DateTime _now = DateTime.UtcNow;

        public HelloWorldJob(ILogger<HelloWorldJob> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var text = $"{_now} - Hello World!";
            _logger.LogInformation(text);
            Console.WriteLine(text);
            return Task.CompletedTask;
        }
    }
}