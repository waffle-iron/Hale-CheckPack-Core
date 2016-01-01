using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HaleLib.Modules;
using HaleLib.Modules.Checks;
using HaleLib.Modules.Info;
using System.Management;

namespace Hale.Modules
{
    /// <summary>
    /// All checks need to realize the interface ICheck.
    /// </summary>
    public class CpuModule : Module, ICheckProvider, IInfoProvider
    {

        public override string Name { get; } = "CPU Usage";
        public override string Platform { get; } = "Windows";
        public override decimal TargetApi { get; } = 1.2M;
        public override string Identifier { get; } = "com.itshale.core.cpu";

        Dictionary<string, ModuleFunction> IModuleProviderBase.Functions { get; set; }
            = new Dictionary<string, ModuleFunction>();

        public CheckResult DefaultCheck(CheckSettings settings)
        {
            CheckResult cr = new CheckResult();

            try
            {
                PerformanceCounter cpuCounter;
                cpuCounter = new PerformanceCounter();

                cpuCounter.CategoryName = "Processor";
                cpuCounter.CounterName = "% Processor Time";
                cpuCounter.InstanceName = "_Total";

                int numSamples = settings.ContainsKey("samples") ? int.Parse(settings["samples"]) : 10;
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

                cr.RawValues.Add( new DataPoint("CpuUsage", cpuPercentage) );

                cr.SetThresholds(cpuPercentage, settings.Thresholds);

                cr.Message = $"CPU usage average: {cpuPercentage.ToString("p1")}, min: {sampleMin.ToString("p1")}, max: {sampleMax.ToString("p1")}";

                cr.RanSuccessfully = true;
            }

            catch(Exception x)
            {
                cr.ExecutionException = x;
                cr.RanSuccessfully = false;
                cr.Message = "The check failed to execute due to exception: " + x.Message;
            }

            return cr;
        }

        public CheckResult PerformanceCheck(CheckSettings settings)
        {
            CheckResult cr = new CheckResult();

            try
            {
                PerformanceCounter cpuCounter;
            cpuCounter = new PerformanceCounter();

            cpuCounter.CategoryName = "Processor Information";
            cpuCounter.CounterName = "% Processor Performance";
            cpuCounter.InstanceName = "_Total";

            int numSamples = settings.ContainsKey("samples") ? int.Parse(settings["samples"]) : 10;
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

                cr.RawValues.Add(new DataPoint("CpuPerformance", cpuPercentage));

                cr.SetThresholds(cpuPercentage, settings.Thresholds);

                cr.Message = $"CPU performance average: {cpuPercentage.ToString("p1")}, min: {sampleMin.ToString("p1")}, max: {sampleMax.ToString("p1")}";

                cr.RanSuccessfully = true;
            }

            catch (Exception x)
            {
                cr.ExecutionException = x;
                cr.RanSuccessfully = false;
                cr.Message = "The check failed to execute due to exception: " + x.Message;
            }

            return cr;

        }

        public InfoFunctionResult DefaultInfo(InfoSettings settings)
        {
            var result = new InfoFunctionResult();
            try
            {
                var cpus = GetCPUProperties(new byte[] { }, new[] { "MaxClockSpeed", "NumberOfLogicalProcessors", "NumberOfCores", "Name", "Manufacturer" });
                foreach(var cpu in cpus)
                {
                    result.InfoResults.Add(cpu.Key.ToString(), new InfoResult()
                    {
                        RanSuccessfully = true,
                        Items = cpu.Value
                    });
                }
                result.Message = "Successfully retrieved default CPU info.";
                result.RanSuccessfully = true;
            }
            catch (Exception x)
            {
                result.FunctionException = x;
                result.Message = $"Cannot get info: {x.Message}.";
            }

            return result;
        }

        Dictionary<byte, Dictionary<string, string>> GetCPUProperties(byte[] targets, string[] filter)
        {
            var cpus = new Dictionary<byte,Dictionary<string, string>>();

            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            var moc = searcher.Get();

            byte cpuId = 0;

            foreach (var mo in moc)
            {
                var items = new Dictionary<string, string>();
                foreach (var p in mo.Properties)
                {
                    if (p.Value != null && filter.Contains(p.Name))
                        items.Add(p.Name, p.Value.ToString().TrimEnd());
                }
                if(targets.Length == 0 || targets.Contains(cpuId))
                    cpus.Add(cpuId, items);
                cpuId++;
            }
            return cpus;
        }

        public void InitializeCheckProvider(CheckSettings settings)
        {
            this.AddSingleResultCheckFunction(DefaultCheck);
            this.AddSingleResultCheckFunction("usage", DefaultCheck);
            this.AddSingleResultCheckFunction("performance", PerformanceCheck);
        }

        public void InitializeInfoProvider(InfoSettings settings)
        {
            this.AddInfoFunction(DefaultInfo);
        }
    }
}
