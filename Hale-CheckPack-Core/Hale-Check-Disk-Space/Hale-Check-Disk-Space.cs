using System;
using System.Collections.Generic;
using System.IO;
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
            Name = "Disk Space";
            Author = "Simon Aronsson, It's Hale";
            Platform = "Windows";
            Version = (decimal)0.01;
            TargetCore = (decimal)0.01;
        }


        /// <summary>
        /// Executes the check and returns a Response instance. This is then serialized in
        /// the agent to JSON. This method must always be named Execute, and should always return a
        /// response following the stated standard. However, you are free to add as much logic as your
        /// use case requires in other classes/methods.
        /// 
        /// Any checks not adhering to this standard will not be merged into the checkpacks.
        /// </summary>
        /// <param name="crit">The critical threshold for each object that is checked.</param>
        /// <param name="warn">The warning threshold for each object that is checked.</param>
        /// <param name="origin">A hash representing the host that requested the information.</param>
        public Response Execute(string origin, long warn = 20, long crit = 10)
        {
            Response response = new Response();
            
            response.Origin = origin;
            response.Status = (int)Status.OK;
            try {
                DriveInfo[] drives = DriveInfo.GetDrives();

                foreach (DriveInfo drive in drives)
                {
                    long diskPercentage = (100 * drive.TotalFreeSpace / drive.TotalSize);

                    response.Status = (diskPercentage <= crit ? (int)Status.Critical : (diskPercentage <= warn && response.Status == (int)Status.OK ? (int)Status.Warning: (int)Status.OK));
                    response.Text.Add(drive.Name + " " + ConvertToStorageSizes(drive.TotalFreeSpace) + " of " + ConvertToStorageSizes(drive.TotalSize) + " free (" + diskPercentage + "%)");
                    response.Performance.Add(new PerformancePoint(drive.Name + " Free %", diskPercentage));
                
                }
            }
            catch (Exception e)
            {
                response.Text.Add("Failed to execute check.");
                response.Text.Add(e.Message + "\n" + e.StackTrace);
                response.Status = (int)Status.Critical;
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


            if (remainder / TER >= 1)
            {
                builder.Append(remainder / TER + "TB ");
                remainder = remainder % TER;

            }
            else if (remainder / GIG >= 1)
            {
                builder.Append(remainder / GIG + "GB ");
                remainder = remainder % GIG;
            }
            else if (remainder / MEG >= 1)
            {
                builder.Append(remainder / MEG + "MB ");
                remainder = remainder % MEG;
            }
            else if (remainder / KIL >= 1)
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
