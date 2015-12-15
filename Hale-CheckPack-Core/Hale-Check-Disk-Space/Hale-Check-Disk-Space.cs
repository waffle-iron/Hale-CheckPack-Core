using Hale_Lib.Checks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using static Hale_Lib.Utilities.StorageUnitFormatter;

namespace Hale.Checks
{
    /// <summary>
    /// This is a mandatory class that should contain all information regarding the check. This will be instantiated and added to the dynamic list in the Agent.
    /// </summary>
    public class DiskSpaceCheck: ICheck
    {

        /// <summary>
        /// The name of the check. This will be visible in the Web UI.
        /// For example: "System Uptime"
        /// </summary>
        public string Name { get; } = "Disk Space";

        /// <summary>
        /// Person and organization (opt) that developed the check.
        /// </summary>
        public string Author { get; } = "hale project";

        /// <summary>
        /// Internal version of the check itself.
        /// </summary>
        public Version Version { get; } = new Version(0, 1, 1);

        /// <summary>
        /// What platform is this check targeted at?
        /// Might be a specific release of Windows, Linux, OS/400 etc.
        /// </summary>
        public string Platform { get; } = "Windows";

        /// <summary>
        /// </summary>
        public decimal TargetApi { get; } = 0.1M;

        /// <summary>
        /// </summary>
        public bool ParallelExecution { get; } = true;

        /// <summary>
        /// Set all the attributes above directly in the constructor.
        /// </summary>
        public DiskSpaceCheck()
        {

        }

        /// <summary>
        /// Executes the check and returns a Response instance. This is then serialized in
        /// the agent to JSON. This method must always be named Execute, and should always return a
        /// response following the stated standard. However, you are free to add as much logic as your
        /// use case requires in other classes/methods.
        /// 
        /// Any checks not adhering to this standard will not be merged into the checkpacks.
        /// </summary>
        public CheckResult Execute(CheckTargetSettings settings)
        {
            CheckResult result = new CheckResult();

            var sb = new StringBuilder();

            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();


                bool found = false;
                foreach (DriveInfo drive in drives)
                {
               
                    if (drive.Name.ToLower() == settings.Target.ToLower())
                    {
                        float diskPercentage = ((float)drive.TotalFreeSpace / drive.TotalSize);

                        result.Message = $"{drive.Name} ({drive.VolumeLabel}) {HumanizeStorageUnit(drive.TotalFreeSpace)}of {HumanizeStorageUnit(drive.TotalSize)}free ({diskPercentage.ToString("P1")}).";

                        result.RawValues.Add(new DataPoint("freePercentage", diskPercentage));
                        result.RawValues.Add(new DataPoint("freeBytes", drive.TotalSize - drive.TotalFreeSpace));

                        result.SetThresholds(diskPercentage, settings.Thresholds);

                        result.RanSuccessfully = true;

                        found = true;

                        break;
                    }
                }
                if(!found)
                {
                    result.Message = $"Could not retrieve disk space for disk \"{settings.Target}\"";
                    result.CheckException = new Exception("Target not found.");
                    result.RanSuccessfully = false;
                }
            }
            catch (Exception e)
            {
                result.Message = "Failed to get disk space.";
                result.CheckException = e;
                result.RanSuccessfully = false;
            }

            return result;
        }

        public void Initialize(CheckSettings settings)
        {
            
        }
    }
}
