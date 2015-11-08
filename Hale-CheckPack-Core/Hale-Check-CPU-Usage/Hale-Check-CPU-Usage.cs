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

namespace Hale.Agent
{
    /// <summary>
    /// All checks need to realize the interface ICheck.
    /// </summary>
    public class Check : ICheck
    {

        public string Name
        {
            get {
                return "Disk Usage";
            }
        }
        public string Author
        {
            get
            {
                return "Simon Aronsson";
            }
            
        }

        public decimal TargetApi
        {
            get
            {
                return 0.1M;
            }
        }


        public Version Version
        {
            get
            {
                return new Version (0, 1, 1);
            }
        }

        /// <summary>
        /// What platform is this check targeted at?
        /// Might be a specific release of Windows, Linux, OS/400 etc.
        /// </summary>
        public string Platform
        {
            get
            {
                return "Windows";
            }
            
        }

        /// <summary>
        /// What Hale Core was this check developed for?
        /// This is to avoid compability issues.
        /// </summary>
        public decimal TargetAPI
        {
            get
            {
                return 0.01M;
            }
            
        }

        public Response Execute(string origin, long warn, long crit)
        {
            Response response = new Response();
            response.Status = (int) Status.OK;

            try
            {
                PerformanceCounter cpuCounter;
                cpuCounter = new PerformanceCounter();

                cpuCounter.CategoryName = "Processor";
                cpuCounter.CounterName = "% Processor Time";
                cpuCounter.InstanceName = "_Total";

                cpuCounter.NextValue();
                Thread.Sleep(500);

                float cpuPercentage = cpuCounter.NextValue();

                response.Metrics.Add(
                    new Metric()
                    {
                        Name = "CPU Usage",
                        Unit = (int)MetricUnits.Percentage,
                        Value = cpuPercentage
                    }
                );

                if (cpuPercentage > crit)
                {
                    response.Status = (int)Status.Critical;
                    response.Text.Add("CPU Usage has exceeded the critical threshold (" + cpuPercentage + "% > " + crit + "%)");
                }
                else if (cpuPercentage > warn)
                {
                    if (response.Status != (int)Status.Critical)
                        response.Status = (int)Status.Warning;

                    response.Text.Add("CPU Usage has exceeded the warning threshold (" + cpuPercentage + "% > " + warn + "%)");
                }
                else
                {
                    response.Text.Add("CPU is currently working at " + cpuPercentage + "% of the total capacity.");
                }
            }

            catch
            {
                response = new Response();
                response.Status = (int) Status.Critical;
                response.Successful = false;
                response.Text.Add("The check failed to execute.");
            }
            return response;
        }

        
        
        

    }
}
