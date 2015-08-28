using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    float percentage = FetchDiskFreePercentage(drive);
                    response.Metrics.Add(new Metric() { Name = drive.Name, Unit = (int)MetricUnits.Percentage, Value = percentage});
                    if (percentage < crit)
                    {
                        response.Text.Add(drive.Name + " has exceeded the critical limit (" + crit + "%)");
                        response.Status = (int)Status.Critical;
                    }
                        
                    else if (percentage < warn)
                    {
                        if (response.Status != (int) Status.Critical)
                            response.Status = (int) Status.Warning;

                        response.Text.Add(drive.Name + " has exceeded the warning limit (" + warn + " %)");
                    }
                    else
                    {
                        response.Text.Add(drive.Name + " is OK.");
                    }
                        

                }
            }
            return response;
        }

        internal float FetchDiskFreePercentage(DriveInfo drive)
        {
            return (drive.TotalFreeSpace / drive.TotalSize);

        }
        
        

    }
}
