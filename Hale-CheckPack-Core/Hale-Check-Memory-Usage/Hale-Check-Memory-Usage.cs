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
    public class DiskSpaceCheck : Module, ICheckProvider, IInfoProvider
    {

        public new string Name { get; } = "Memory Usage";

        public new string Author { get; } = "Hale Project";

        public override string Identifier { get; } = "com.itshale.core.memory";

        public new Version Version { get; } = new Version (0, 1, 1);

        public override string Platform { get; } = "Windows";

        public new decimal TargetApi { get; } = 1.1M;

        Dictionary<string, ModuleFunction> IModuleProviderBase.Functions { get; set; }
            = new Dictionary<string, ModuleFunction>();


        public CheckResult DefaultCheck(CheckTargetSettings settings)
        {
            CheckResult result = new CheckResult();

            try
            {
                PerformanceCounter ramPercentage = new PerformanceCounter()
                {
                    CounterName = "% Committed Bytes in Use",
                    CategoryName = "Memory"
                };

                ramPercentage.NextValue();

                float freePercentage = 1.0F - (ramPercentage.NextValue() / 100.0F);


                var ci = new Microsoft.VisualBasic.Devices.ComputerInfo();

                ulong memoryTotal = ci.TotalPhysicalMemory;

                // Note: ci.AvailablePhysicalMemory does not return "accurate" data -NM
                //ulong memoryFree = ci.AvailablePhysicalMemory;

                // Hack: Using this calculated approximation for now. -NM
                ulong memoryFree = (ulong) Math.Round(memoryTotal * (freePercentage));

                result.Message = $"RAM Usage: {HumanizeStorageUnit(memoryFree)}free of total {HumanizeStorageUnit(memoryTotal)}({(freePercentage).ToString("P1")})";

                // Raw value is percent of free RAM (0.0 .. 1.0)
                result.RawValues.Add(new DataPoint() { DataType = "freePercentage", Value = freePercentage });
                result.RawValues.Add(new DataPoint() { DataType = "freeBytes", Value = (float)memoryFree });

                result.SetThresholds(freePercentage, settings.Thresholds);

                result.RanSuccessfully = true;

            }
            catch (Exception e)
            {
                result.Message = "Could not get RAM information.";
                result.RanSuccessfully = false;
                result.ExecutionException = e;
            }

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
            this.AddCheckFunction("usage", DefaultCheck);
        }

        public void InitializeInfoProvider(InfoSettings settings)
        {
            this.AddInfoFunction(DefaultInfo);
            this.AddInfoFunction("sizes", DefaultInfo);
        }

    }
}
