﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hale.Agent
{
    /// <summary>
    /// This is a mandatory class that should contain all information regarding the check. This will be instantiated and added to the dynamic list in the Agent.
    /// </summary>
    public class Check : ICheck
    {

        public string Name
        {
            get {
                return "Memory Usage";
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

            try
            {
                PerformanceCounter ramPercentage = new PerformanceCounter()
                {
                    CounterName = "% Committed Bytes in Use",
                    CategoryName = "Memory"
                };

                ramPercentage.NextValue();

                int ramOut = (int)ramPercentage.NextValue();
                long total = (long)new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;

                response.Text.Add("Total RAM: " + ConvertToStorageSizes(total) + " (" + ramOut + "% free)");
                response.Performance.Add(new PerformancePoint("RAM", ramOut));

                if (ramOut <= crit)
                    response.Status = (int)Status.Critical;
                else if (ramOut <= warn)
                    response.Status = (int)Status.Warning;
                else
                    response.Status = (int)Status.OK;
            }
            catch (Exception e)
            {
                response.Status = (int)Status.Critical;
                response.Text.Add(e.Message);
                response.Text.Add(e.StackTrace);
            }

            return response;
        }

        
    }

    public interface ICheck
    {
        string Name
        {
            get;
        }
        string Author { get; }
        Version Version { get; }

        string Platform { get; }

        Decimal TargetApi { get; }

        public Response Execute(string origin, long warn = 0, long crit = 0);
    }
}
