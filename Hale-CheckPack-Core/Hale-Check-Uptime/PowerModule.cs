using HaleLib.Modules;
using HaleLib.Modules.Info;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using static HaleLib.Utilities.TimeSpanFormatter;

namespace Hale.Modules
{
    // Todo: Change this module to Power perhaps? -NM 2015-12-30

    public class UptimeCheck: Module, IInfoProvider
    {

        public new string Name { get; } = "Power";

        public new string Author { get; } = "hale project";

        public override string Identifier { get; } = "com.itshale.core.power";

        public override string Platform { get; } = "Windows";

        public new decimal TargetApi { get; } = 1.2M;

        Dictionary<string, ModuleFunction> IModuleProviderBase.Functions { get; set; }
            = new Dictionary<string, ModuleFunction>();

        public InfoResult DefaultInfo(InfoSettings settings)
        {
            var result = new InfoResult();

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
            this.AddSingleResultInfoFunction(DefaultInfo);
            this.AddSingleResultInfoFunction("uptime", DefaultInfo);
        }

    }
}
