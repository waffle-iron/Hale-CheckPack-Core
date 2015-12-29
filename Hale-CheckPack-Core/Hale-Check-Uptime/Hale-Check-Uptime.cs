using HaleLib.Modules;
using HaleLib.Modules.Info;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using static HaleLib.Utilities.TimeSpanFormatter;

namespace Hale.Checks
{
    // Todo: Change this module to Power perhaps? -NM 2015-12-30

    public class UptimeCheck: Module, IInfoProvider
    {

        public new string Name { get; } = "System Uptime";

        public new string Author { get; } = "hale project";

        public override string Identifier { get; } = "com.itshale.core.uptime";

        public new Version Version { get; } = new Version(0, 1, 1);

        public override string Platform { get; } = "Windows";

        public decimal TargetApi { get; } = 1.1M;

        Dictionary<string, ModuleFunction> IModuleProviderBase.Functions { get; set; }
            = new Dictionary<string, ModuleFunction>();

        public InfoResult DefaultInfo(InfoTargetSettings settings)
        {
            var result = new InfoResult(settings.Target);

            try {
                TimeSpan uptime = new TimeSpan();

                float _raw;

                using (var utCounter = new System.Diagnostics.PerformanceCounter("System", "System Up Time"))
                {
                    utCounter.NextValue();
                    _raw = utCounter.NextValue();
                    uptime = TimeSpan.FromSeconds(_raw);
                }

                result.Items.Add("uptimeSeconds", _raw.ToString());

                result.Message = "Uptime: " + HumanizeTimeSpan(uptime);

                result.RanSuccessfully = true;
            }
            catch (Exception x)
            {
                result.ExecutionException = x;
                result.RanSuccessfully = false;
            }

            return result;
        }

        public void InitializeInfoProvider(InfoSettings settings)
        {
            this.AddInfoFunction(DefaultInfo);
            this.AddInfoFunction("uptime", DefaultInfo);
        }

    }
}
