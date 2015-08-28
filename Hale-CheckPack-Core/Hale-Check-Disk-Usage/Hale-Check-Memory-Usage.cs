using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hale_Lib.Responses;
using Hale_Lib;
using Microsoft.VisualBasic.Devices;

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
                response.Metrics.Add(new Metric() { Name = "RAM", Unit = (int)MetricUnits.Percentage, Value = ramOut });

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

        private string ConvertToStorageSizes(long p)
        {
            const long TER = 1099511627776;
            const long GIG = 1073741824;
            const long MEG = 1048576;
            const long KIL = 1024;


            StringBuilder builder = new StringBuilder();

            long remainder = p;


            if (remainder >= TER)
            {
                builder.Append(remainder / TER + "TB ");
                remainder = remainder % TER;

            }
            else if (remainder >= GIG)
            {
                builder.Append(remainder / GIG + "GB ");
                remainder = remainder % GIG;
            }
            else if (remainder >= MEG)
            {
                builder.Append(remainder / MEG + "MB ");
                remainder = remainder % MEG;
            }
            else if (remainder >= KIL)
            {
                builder.Append(remainder / KIL + "KB ");
                remainder = remainder % KIL;
            }
            else
                builder.Append(remainder + "B ");

            return builder.ToString();
        }
        
    }
}
