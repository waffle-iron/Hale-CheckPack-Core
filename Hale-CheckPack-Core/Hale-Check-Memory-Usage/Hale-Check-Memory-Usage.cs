using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hale_Lib.Responses;
using Hale_Lib;
using Hale_Lib.Checks;

using static Hale_Lib.Utilities.StorageUnitFormatter;

namespace Hale.Checks
{
    /// <summary>
    /// All checks need to realize the interface ICheck.
    /// </summary>
    public class MemoryUsageCheck : ICheck
    {

        public string Name { get; } = "Memory Usage";

        public string Author { get; } = "Simon Aronsson";

        public Version Version { get; } = new Version (0, 1, 1);

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

        public CheckResult Execute(CheckTargetSettings settings)
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


                result.RawValues = new List<DataPoint>();

                // Raw value is percent of free RAM (0.0 .. 1.0)
                result.RawValues.Add(new DataPoint() { DataType = "freePercentage", Value = freePercentage });
                result.RawValues.Add(new DataPoint() { DataType = "freeBytes", Value = (float)memoryFree });


                result.Critical = (freePercentage <= settings.Thresholds.Critical);
                result.Warning = (freePercentage <= settings.Thresholds.Warning);
                result.RanSuccessfully = true;

            }
            catch (Exception e)
            {
                result.Message = "Could not get RAM information.";
                result.RanSuccessfully = false;
                result.CheckException = e;
            }

            return result;
        }
        
    }
}
