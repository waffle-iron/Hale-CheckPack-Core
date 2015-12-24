using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hale_Lib.Responses;
using Hale_Lib;
using Hale_Lib.Checks;

namespace Hale.Agent
{
    /// <summary>
    /// All checks need to realize the interface ICheck.
    /// </summary>
    public class CpuUsageCheck : ICheck
    {

        public string Name { get; } = "CPU Usage";

        public string Author { get; } = "Hale Project";

        public Version Version { get; } = new Version(0, 1, 1);

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
            CheckResult cr = new CheckResult();

            try
            {
                PerformanceCounter cpuCounter;
                cpuCounter = new PerformanceCounter();

                cpuCounter.CategoryName = "Processor";
                cpuCounter.CounterName = "% Processor Time";
                cpuCounter.InstanceName = "_Total";

                int numSamples = settings.ContainsKey("samples") ? int.Parse(settings["samples"]) : 40;
                int sampleDelay = settings.ContainsKey("delay") ? int.Parse(settings["delay"]) : 200;

                float sampleSum = 0;
                float sampleMax = 0;
                float sampleMin = 100;

                cpuCounter.NextValue();

                for (int i = 0; i < numSamples; i++)
                {
                    float sample = cpuCounter.NextValue();

                    if (sample > sampleMax)
                        sampleMax = sample;

                    if (sample < sampleMin)
                        sampleMin = sample;

                    sampleSum += sample;
                    Thread.Sleep(sampleDelay + (i * 10));
                }

                sampleMax /= 100;
                sampleMin /= 100;

                float cpuPercentage = (sampleSum / numSamples) / 100;

                cr.RawValues.Add( new DataPoint("CPU Usage", cpuPercentage) );

                cr.SetThresholds(cpuPercentage, settings.Thresholds);

                cr.Message = $"CPU average: {cpuPercentage.ToString("p1")}, min: {sampleMin.ToString("p1")}, max: {sampleMax.ToString("p1")}";

                cr.RanSuccessfully = true;
            }

            catch(Exception x)
            {
                cr.CheckException = x;
                cr.RanSuccessfully = false;
                cr.Message = "The check failed to execute due to exception: " + x.Message;
            }
            return cr;
        }

        public void Initialize(CheckSettings settings)
        {
            
        }
    }
}
