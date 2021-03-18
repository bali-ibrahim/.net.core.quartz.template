using System;
using System.Threading.Tasks;
using Quartz;

namespace NonWebAppTemplate
{
    [DisallowConcurrentExecution]
    public class HelloWorldJob2 : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var asd = new DummyJob();
            const string text = "Hello World 2!";
            Console.WriteLine(text);
            return Task.CompletedTask;
        }
    }
}