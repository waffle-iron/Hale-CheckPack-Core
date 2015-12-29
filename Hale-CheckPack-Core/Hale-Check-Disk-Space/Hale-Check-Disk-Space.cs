using HaleLib.Modules;
using HaleLib.Modules.Checks;
using HaleLib.Modules.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using static HaleLib.Utilities.StorageUnitFormatter;

namespace Hale.Checks
{
    public class DiskSpaceCheck: Module, ICheckProvider, IInfoProvider
    {

        public new string Name { get; } = "Disk Space";

        public new string Author { get; } = "hale project";

        public override string Identifier { get; } = "com.itshale.core.disk";

        public new Version Version { get; } = new Version(0, 1, 1);

        public override string Platform { get; } = "Windows";

        public new decimal TargetApi { get; } = 1.1M;

        Dictionary<string, ModuleFunction> IModuleProviderBase.Functions { get; set; }
            = new Dictionary<string, ModuleFunction>();

        public DiskSpaceCheck()
        {

        }

        public CheckResult DefaultCheck(CheckTargetSettings settings)
        {
            CheckResult result = new CheckResult(settings.Target);

            var sb = new StringBuilder();

            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                var volumeName = settings.Target.ToLower() + ":\\";

                bool found = false;
                foreach (DriveInfo drive in drives)
                {
                    
                    if (drive.Name.ToLower() == volumeName)
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
                    result.ExecutionException = new Exception("Target not found.");
                    result.RanSuccessfully = false;
                }
            }
            catch (Exception e)
            {
                result.Message = "Failed to get disk space.";
                result.ExecutionException = e;
                result.RanSuccessfully = false;
            }

            return result;
        }

        public InfoResult DefaultInfo(InfoTargetSettings settings)
        {
            var result = new InfoResult(settings.Target);
            result.ExecutionException = new NotImplementedException();
            return result;
        }

        public void InitializeCheckProvider(CheckSettings settings)
        {
            this.AddCheckFunction(DefaultCheck);
            this.AddCheckFunction("usage", DefaultCheck);
        }

        public void InitializeInfoProvider(InfoSettings settings)
        {
            this.AddInfoFunction(DefaultInfo);
            this.AddInfoFunction("sizes", DefaultInfo);
        }
    }
}
