using HaleLib.Modules;
using HaleLib.Modules.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Hale.Modules
{
    partial class ServiceModule
    {
        public CheckFunctionResult ServiceRunningCheck(CheckSettings settings)
        {
            var cfr = new CheckFunctionResult();

            try
            {
                IEnumerable<string> services = settings.Targetless ? _getAutomaticServices() : settings.Targets;
                foreach (var service in services)
                {
                    var cr = new CheckResult();
                    try
                    {
                        ServiceController sc = new ServiceController(service);

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
                        cr.ExecutionException = x;
                        cr.RanSuccessfully = false;
                    }
                    cfr.CheckResults.Add(service, cr);
                }
                cfr.RanSuccessfully = true;
            }
            catch (Exception x)
            {
                cfr.FunctionException = x;
                cfr.Message = x.Message;
                cfr.RanSuccessfully = false;
            }

            return cfr;
        }

        #region WMI
        private string[] _getAutomaticServices()
        {
            var services = new List<string>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Service WHERE StartMode = 'Auto'");

            foreach (ManagementObject mo in searcher.Get())
            {
                services.Add(mo["Name"].ToString());
            }

            return services.ToArray();
        }
        #endregion
    }
}
