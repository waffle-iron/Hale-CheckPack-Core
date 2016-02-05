using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using HaleLib;
using HaleLib.Modules;
using HaleLib.Modules.Checks;
using HaleLib.Modules.Info;

using static HaleLib.Utilities.StorageUnitFormatter;


namespace Hale.Checks
{
    /// <summary>
    /// All checks need to realize the interface ICheck.
    /// </summary>
    public class DnsResolvability: Module, ICheckProvider, IInfoProvider
    {

        public new string Name              { get; } = "DNS Resolvability Test";
        public new string Author            { get; } = "Hale Project";
        public override string Identifier   { get; } = "com.itshale.core.paging";
        public new Version Version          { get; } = new Version (0, 1, 1);
        public override string Platform     { get; } = "Windows";
        public new decimal TargetApi        { get; } = 1.2M;

        Dictionary<string, ModuleFunction> IModuleProviderBase.Functions { get; set; }
            = new Dictionary<string, ModuleFunction>();


        public CheckResult DefaultCheck(CheckSettings settings)
        {
            
            if (settings.Targetless) { 
                return new CheckResult()
                {
                    Message = $"No targets where configured.",
                    RanSuccessfully = false
                };
            }

            CheckResult result = new CheckResult();
            int failed = 0;

            foreach (string target in settings.Targets)
             {
                try
                {
                    IPAddress[] addresses = Dns.GetHostAddresses(target);
                    if (addresses.Length == 0)
                        failed++;
                    result.RanSuccessfully = true;
                }
                catch (Exception e)
                {
                    failed++;
                }
                    
            }

            if (failed == settings.Targets.Count)
            {
                result.RanSuccessfully = false;

            }

            int successfulPercentage = (failed / settings.Targets.Count);

            result.Message = $"{successfulPercentage}% of " + settings.Targets.Count + " DNS Resolves was succesful.";
            result.RawValues.Add(new DataPoint() { DataType = "successfulPercentage", Value = successfulPercentage});
            result.RanSuccessfully = true;

            return result;
        }

        public InfoResult DefaultInfo(InfoSettings settings)
        {
            var result = new InfoResult();
            result.ExecutionException = new NotImplementedException();
            return result;
        }

        public void InitializeCheckProvider(CheckSettings settings)
        {
            this.AddSingleResultCheckFunction(DefaultCheck);
            this.AddSingleResultCheckFunction("usage", DefaultCheck);
        }

        public void InitializeInfoProvider(InfoSettings settings)
        {
            this.AddSingleResultInfoFunction(DefaultInfo);
            this.AddSingleResultInfoFunction("sizes", DefaultInfo);
        }

    }
}
