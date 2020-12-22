using System.Collections.Generic;

namespace AspNetCore.Scheduler.Quartz
{
    public class QuartzConfig
    {
        public string ServiceFolder { get; set; }
        public IDictionary<string, string> Jobs { get; set; }
    }
}