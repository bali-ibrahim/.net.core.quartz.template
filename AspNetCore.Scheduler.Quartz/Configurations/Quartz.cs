using System.Collections.Generic;

namespace AspNetCore.Scheduler.Quartz.Configurations
{
    public class Quartz
    {
        public IDictionary<string, IEnumerable<Job>> Triggers { get; set; }
    }
}