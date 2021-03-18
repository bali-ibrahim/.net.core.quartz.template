using System.Threading.Tasks;
using Quartz;

namespace AspNetCore.Scheduler.Quartz
{
    internal class DummyJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Delay(100);
        }
    }
}