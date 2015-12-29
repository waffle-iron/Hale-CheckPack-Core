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

        public CheckResult DefaultCheck(CheckTargetSettings settings)
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
                cr.ExecutionException = x;
                cr.Message = x.Message;
                cr.RanSuccessfully = false;
            }

            return cr;
        }

        public ActionResult StartAction(ActionTargetSettings settings)
        {
            var result = new ActionResult();
            result.Target = settings.Target;
            result.ExecutionException = new NotImplementedException();
            return result;
        }

        public InfoResult DefaultInfo(InfoTargetSettings settings)
        {
            var result = new InfoResult(settings.Target);
            result.ExecutionException = new NotImplementedException();
            return result;
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
