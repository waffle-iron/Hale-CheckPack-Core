using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public class PagingUsage: Module, ICheckProvider, IInfoProvider
    {

        public new string Name              { get; } = "Paging File Usage %";
        public new string Author            { get; } = "Hale Project";
        public override string Identifier   { get; } = "com.itshale.core.paging";
        public new Version Version          { get; } = new Version (0, 1, 1);
        public override string Platform     { get; } = "Windows";
        public new decimal TargetApi        { get; } = 1.2M;

        Dictionary<string, ModuleFunction> IModuleProviderBase.Functions { get; set; }
            = new Dictionary<string, ModuleFunction>();


        public CheckResult DefaultCheck(CheckSettings settings)
        {
            CheckResult result = new CheckResult();

            try
            {
                PerformanceCounter pagefileUsagePercent = new PerformanceCounter()
                {
                    CounterName = "% Usage",
                    CategoryName = "Paging File",
                    InstanceName =  "_Total"
                   
                };

                pagefileUsagePercent.NextValue();

                float usagePercent = pagefileUsagePercent.NextValue();

                result.Message = $"Paging File Usage: {usagePercent}% of total";
                result.RawValues.Add(new DataPoint() { DataType = "usagePercent", Value = usagePercent });
                result.RanSuccessfully = true;

            }
            catch (Exception e)
            {
                result.Message = "Could not fetch swap usage information.";
                result.RanSuccessfully = false;
                result.ExecutionException = e;
            }

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
