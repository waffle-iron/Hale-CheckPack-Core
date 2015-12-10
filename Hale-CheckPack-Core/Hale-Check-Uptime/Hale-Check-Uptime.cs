using Hale_Lib.Checks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using static Hale_Lib.Utilities.TimeSpanFormatter;

namespace Hale.Checks
{
    /// <summary>
    /// This is a mandatory class that should contain all information regarding the check. This will be instantiated and added to the dynamic list in the Agent.
    /// </summary>
    public class UptimeCheck: ICheck
    {

        /// <summary>
        /// The name of the check. This will be visible in the Web UI.
        /// For example: "System Uptime"
        /// </summary>
        public string Name { get; } = "System Uptime";

        /// <summary>
        /// Person and organization (opt) that developed the check.
        /// </summary>
        public string Author { get; } = "hale project";

        /// <summary>
        /// Internal version of the check itself.
        /// </summary>
        public Version Version { get; } = new Version(0, 1, 1);

        /// <summary>
        /// What platform is this check targeted at?
        /// Might be a specific release of Windows, Linux, OS/400 etc.
        /// </summary>
        public string Platform { get; } = "Windows";

        /// <summary>
        /// </summary>
        public decimal TargetApi { get; } = 0.1M;

        /// <summary>
        /// </summary>
        public bool ParallelExecution { get; } = true;

        /// <summary>
        /// Set all the attributes above directly in the constructor.
        /// </summary>
        public UptimeCheck()
        {
        }


        /// <summary>
        /// Executes the check and returns a Response instance. This is then serialized in
        /// the agent to JSON. This method must always be named Execute, and should always return a
        /// response following the stated standard. However, you are free to add as much logic as your
        /// use case requires in other classes/methods.
        /// 
        /// Any checks not adhering to this standard will not be merged into the checkpacks.
        /// </summary>
        /// <param name="settings"></param>
        public CheckResult Execute(CheckTargetSettings settings)
        {
            CheckResult result = new CheckResult();

            try {
                TimeSpan uptime = new TimeSpan();

                float _raw;

                using (var utCounter = new System.Diagnostics.PerformanceCounter("System", "System Up Time"))
                {
                    utCounter.NextValue();
                    _raw = utCounter.NextValue();
                    uptime = TimeSpan.FromSeconds(_raw);
                }

                result.RawValues = new List<DataPoint>();
                result.RawValues.Add(new DataPoint("uptimeSeconds", _raw));

                result.Message = "Uptime: " + HumanizeTimeSpan(uptime);

                result.Warning = _raw > settings.Thresholds.Warning;
                result.Critical = _raw > settings.Thresholds.Critical;

                result.RanSuccessfully = true;
            }
            catch (Exception x)
            {
                result.CheckException = x;
                result.RanSuccessfully = false;
            }

            return result;
        }

    }
}
