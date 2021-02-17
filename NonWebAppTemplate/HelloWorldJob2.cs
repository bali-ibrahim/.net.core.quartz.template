using System;
using System.Threading.Tasks;
using Quartz;

namespace Scheduler.ServiceTemplate
{
    [DisallowConcurrentExecution]
    public class HelloWorldJob2 : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            const string text = "Hello World!";
            Console.WriteLine(text);
            return Task.CompletedTask;
        }
    }
}