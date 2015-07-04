using System;
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
    public class Check
    {

        /// <summary>
        /// The name of the check. This will be visible in the Web UI.
        /// For example: "System Uptime"
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// Person and organization (opt) that developed the check.
        /// </summary>
        public string Author
        {
            get;
            set;
        }

        /// <summary>
        /// Internal version of the check itself.
        /// </summary>
        public decimal Version
        {
            get;
            set;
        }

        /// <summary>
        /// What platform is this check targeted at?
        /// Might be a specific release of Windows, Linux, OS/400 etc.
        /// </summary>
        public string Platform
        {
            get;
            set;
        }

        /// <summary>
        /// What Hale Core was this check developed for?
        /// This is to avoid compability issues.
        /// </summary>
        public decimal TargetCore
        {
            get;
            set;
        }

        /// <summary>
        /// Set all the attributes above directly in the constructor.
        /// </summary>
        public Check()
        {
            Name = "Memory Usage";
            Author = "Simon Aronsson, It's Hale";
            Platform = "Windows";
            Version = 0.01M;
            TargetCore = 0.01M;
        }


        /// <summary>
        /// Executes the check and returns a Response instance. This is then serialized in
        /// the agent to JSON. This method must always be named Execute, and should always return a
        /// response following the stated standard. However, you are free to add as much logic as your
        /// use case requires in other classes/methods.
        /// 
        /// Any checks not adhering to this standard will not be merged into the checkpacks.
        /// </summary>
        /// <param name="crit">Not used for this check, but mandatory</param>
        /// <param name="warn">Not used for this check, but mandatory</param>
        /// <param name="origin">The host requesting the check</param>
        public Response Execute(string origin, long warn = 0, long crit = 0 )
        {
            Response response = new Response();

            try
            {
                PerformanceCounter ramPercentage = new PerformanceCounter()
                {
                    CounterName = "% Commited Bytes in Use",
                    CategoryName = "Memory"
                };

                ramPercentage.NextValue();

                int ramOut = (int)ramPercentage.NextValue();
                long total = (long)new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;

                response.Text.Add("Total RAM: " + ConvertToStorageSizes(total) + " (" + ramOut + "% free)");
                response.Performance.Add(new PerformancePoint("RAM", ramOut));

                if (ramOut <= warn)
                    response.Status = (int)Status.Warning;
                else if (ramOut <= crit)
                    response.Status = (int)Status.Critical;
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
