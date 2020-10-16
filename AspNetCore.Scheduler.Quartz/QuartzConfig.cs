using System.Collections.Generic;

namespace AspNetCore.Scheduler.Quartz
{
    public class QuartzConfig
    {
        public IDictionary<string, string> Jobs { get; set; }
    }
}