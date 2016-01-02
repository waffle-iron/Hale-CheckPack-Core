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
    public partial class ServiceModule: Module, ICheckProvider, IInfoProvider, IActionProvider
    {
        public new string Name { get; } = "Service";

        public new string Author { get; } = "Hale Project";

        public override string Identifier { get; } = "com.itshale.core.service";

        public override string Platform { get; } = "Windows";

        public new decimal TargetApi { get; } = 1.2M;

        Dictionary<string, ModuleFunction> IModuleProviderBase.Functions { get; set; }
            = new Dictionary<string, ModuleFunction>();

        static readonly ServiceControllerStatus[] _criticalStatuses = {
            ServiceControllerStatus.Stopped,
            ServiceControllerStatus.StopPending
        };


        public void InitializeCheckProvider(CheckSettings settings)
        {
            this.AddCheckFunction(ServiceRunningCheck);
            this.AddCheckFunction("running", ServiceRunningCheck);
        }

        public void InitializeInfoProvider(InfoSettings settings)
        {
            this.AddInfoFunction(ListServicesInfo);
            this.AddInfoFunction("list", ListServicesInfo);
        }

        public void InitializeActionProvider(ActionSettings settings)
        {
            this.AddActionFunction("start", StartServiceAction);
            this.AddActionFunction("stop", StopServiceAction);
            this.AddActionFunction("restart", RestartServiceAction);
        }
    }
}
