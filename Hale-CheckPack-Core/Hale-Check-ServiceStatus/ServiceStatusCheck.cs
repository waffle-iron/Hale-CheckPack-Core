using Hale_Lib.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace Hale.Checks
{
    public class ServiceStatusCheck: ICheck
    {
        public string Name { get; } = "Service Status";

        public string Author { get; } = "Hale Project";

        public Version Version { get; } = new Version(0, 1, 1);

        /// <summary>
        /// What platform is this check targeted at?
        /// Might be a specific release of Windows, Linux, OS/400 etc.
        /// </summary>
        public string Platform { get; } = "Windows";

        /// <summary>
        /// What Hale Core was this check developed for?
        /// This is to avoid compability issues.
        /// </summary>
        public decimal TargetApi { get; } = 0.01M;

        public bool ParallelExecution { get; } = false;

        static readonly ServiceControllerStatus[] _criticalStatuses = {
            ServiceControllerStatus.Stopped,
            ServiceControllerStatus.StopPending
        };

        public CheckResult Execute(CheckTargetSettings settings)
        {
            var cr = new CheckResult();

            try {
                ServiceController sc = new ServiceController(settings.Target);

                // Set warning if the service is not running
                cr.Warning = sc.Status != ServiceControllerStatus.Running;

                // Set critical if the service is either stopping or stopped
                cr.Critical = _criticalStatuses.Contains(sc.Status);

                cr.Message = $"Service \"{sc.DisplayName}\" has the status of {sc.Status.ToString()}.";

                cr.RawValues.Add(new DataPoint("status", (int)sc.Status));

                cr.RanSuccessfully = true;
            }
            catch (Exception x)
            {
                cr.CheckException = x;
                cr.Message = x.Message;
                cr.RanSuccessfully = false;
            }

            return cr;
        }

        public void Initialize(CheckSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
