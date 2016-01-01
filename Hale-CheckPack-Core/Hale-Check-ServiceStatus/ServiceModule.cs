using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using HaleLib.Modules;
using HaleLib.Modules.Checks;
using HaleLib.Modules.Actions;
using HaleLib.Modules.Info;
using System.Management;

namespace Hale.Modules
{
    public class ServiceModule: Module, ICheckProvider, IInfoProvider, IActionProvider
    {
        public new string Name { get; } = "Service Status";

        public new string Author { get; } = "Hale Project";

        public new Version Version { get; } = new Version(0, 1, 1);

        public override string Identifier { get; } = "com.itshale.core.service";

        public override string Platform { get; } = "Windows";

        public new decimal TargetApi { get; } = 1.1M;

        Dictionary<string, ModuleFunction> IModuleProviderBase.Functions { get; set; }
            = new Dictionary<string, ModuleFunction>();

        static readonly ServiceControllerStatus[] _criticalStatuses = {
            ServiceControllerStatus.Stopped,
            ServiceControllerStatus.StopPending
        };

        public CheckFunctionResult DefaultCheck(CheckSettings settings)
        {
            var cfr = new CheckFunctionResult();

            try {
                IEnumerable<string> services = settings.Targetless ? _getAutomaticServices() : settings.Targets;
                foreach (var service in services)
                {
                    var cr = new CheckResult();
                    try {
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

        private string[] _getAutomaticServices()
        {
            var services = new List<string>();
            ManagementObjectSearcher searcher =  new ManagementObjectSearcher("SELECT Name FROM Win32_Service WHERE StartMode = 'Auto'");

            foreach (ManagementObject mo in searcher.Get())
            {
                services.Add(mo["Name"].ToString());
            }

            return services.ToArray();
        }

        private Dictionary<string, Dictionary<string, string>> _getAllServices()
        {
            var services = new Dictionary<string, Dictionary<string, string>>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name,StartMode,StartName,Caption,State FROM Win32_Service");

            foreach (ManagementObject mo in searcher.Get())
            {
                var properties = new Dictionary<string, string>();
                foreach(var p in mo.Properties)
                {
                    if (p.Name != "Name")
                    {
                        properties.Add(p.Name, p.Value != null ? p.Value.ToString() : "");
                    }
                }
                services.Add(mo["Name"].ToString(), properties);
            }

            return services;
        }

        private Dictionary<string, Dictionary<string, string>> _getServiceDetails(ICollection<string> targets)
        {
            var services = new Dictionary<string, Dictionary<string, string>>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name,StartMode,StartName,Caption,State,Description FROM Win32_Service");

            foreach (ManagementObject mo in searcher.Get())
            {
                if (!targets.Contains(mo["Name"])) continue;
                var properties = new Dictionary<string, string>();
                foreach (var p in mo.Properties)
                {
                    if (p.Name != "Name")
                    {
                        properties.Add(p.Name, p.Value != null ? p.Value.ToString() : "");
                    }
                }
                services.Add(mo["Name"].ToString(), properties);
            }

            return services;
        }

        public ActionFunctionResult StartAction(ActionSettings settings)
        {
            var afr = new ActionFunctionResult();

            foreach (var kvpTarget in settings.TargetSettings)
            {
                var targetSettings = kvpTarget.Value;
                var target = kvpTarget.Key;
                var result = new ActionResult();
                result.ExecutionException = new NotImplementedException();
                afr.ActionResults.Add(target, result);
            }

            return afr;
        }

        public InfoFunctionResult DefaultInfo(InfoSettings settings)
        {
            var ifr = new InfoFunctionResult();
            try {
                foreach (var service in settings.Targetless ? _getAllServices() : _getServiceDetails(settings.Targets))
                {
                    ifr.InfoResults.Add(service.Key, new InfoResult()
                    {
                        Items = service.Value,
                        RanSuccessfully = true,
                    });
                }
                ifr.RanSuccessfully = true;
                ifr.Message = $"Returned details for {ifr.InfoResults.Count} service(s).";

                if (!settings.Targetless)
                {
                    foreach (var target in settings.Targets)
                    {
                        if (!ifr.InfoResults.ContainsKey(target))
                        {
                            ifr.InfoResults.Add(target, new InfoResult()
                            {
                                Message = $"Could not retrieve information details for service \"{target}\"",
                                ExecutionException = new Exception("Target not found."),
                                RanSuccessfully = false
                            });
                        }
                    }
                }
            }
            catch (Exception x)
            {
                ifr.RanSuccessfully = false;
                ifr.FunctionException = x;
            }
            return ifr;
        }

        public void InitializeCheckProvider(CheckSettings settings)
        {
            this.AddCheckFunction(DefaultCheck);
            this.AddCheckFunction("running", DefaultCheck);
        }

        public void InitializeInfoProvider(InfoSettings settings)
        {
            this.AddInfoFunction(DefaultInfo);
            this.AddInfoFunction("list", DefaultInfo);
        }

        public void InitializeActionProvider(ActionSettings settings)
        {
            this.AddActionFunction("start", StartAction);
        }
    }
}
