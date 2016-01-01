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

        public new string Name { get; } = "Storage";

        public new string Author { get; } = "Hale Project";

        public override string Identifier { get; } = "com.itshale.core.storage";

        public override string Platform { get; } = "Windows";

        public new decimal TargetApi { get; } = 1.2M;

        Dictionary<string, ModuleFunction> IModuleProviderBase.Functions { get; set; }
            = new Dictionary<string, ModuleFunction>();

        public DiskSpaceCheck()
        {

        }

        public CheckFunctionResult CheckUsage(CheckSettings settings)
        {
            CheckFunctionResult result = new CheckFunctionResult();

            var sb = new StringBuilder();

            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();

                foreach (DriveInfo drive in drives)
                {
                    var driveName = drive.Name.ToLower().Replace(":\\", "");
                    if (settings.Targetless || settings.Targets.Contains(driveName))
                    {
                        float diskPercentage = ((float)drive.TotalFreeSpace / drive.TotalSize);

                        var cr = new CheckResult();
                        cr.Target = driveName;

                        cr.Message = $"{drive.Name} ({drive.VolumeLabel}) {HumanizeStorageUnit(drive.TotalFreeSpace)}of {HumanizeStorageUnit(drive.TotalSize)}free ({diskPercentage.ToString("P1")}).";

                        cr.RawValues.Add(new DataPoint("freePercentage", diskPercentage));
                        cr.RawValues.Add(new DataPoint("freeBytes", drive.TotalSize - drive.TotalFreeSpace));

                        cr.SetThresholds(diskPercentage, settings.Thresholds);

                        cr.RanSuccessfully = true;

                        result.CheckResults.Add(driveName, cr);
                    }
                }

                if (!settings.Targetless)
                {
                    foreach (var target in settings.Targets)
                    {
                        if (!result.CheckResults.ContainsKey(target))
                        {
                            result.CheckResults.Add(target, new CheckResult()
                            {
                                Message = $"Could not retrieve disk space for disk \"{target}\"",
                                ExecutionException = new Exception("Target not found."),
                                RanSuccessfully = false
                            });
                        }
                    }
                }
                result.Message = $"Retrieved space usage for {result.CheckResults.Count} drive(s).";
                result.RanSuccessfully = true;
            }
            catch (Exception e)
            {
                result.Message = "Failed to get disk space usage.";
                result.FunctionException = e;
                result.RanSuccessfully = false;
            }

            return result;
        }

        public InfoFunctionResult InfoListVolumes(InfoSettings settings)
        {
            var result = new InfoFunctionResult();
            result.FunctionException = new NotImplementedException();
            return result;
        }

        public void InitializeCheckProvider(CheckSettings settings)
        {
            this.AddCheckFunction(CheckUsage);
            this.AddCheckFunction("usage", CheckUsage);
        }

        public void InitializeInfoProvider(InfoSettings settings)
        {
            this.AddInfoFunction(InfoListVolumes);
            this.AddInfoFunction("list", InfoListVolumes);
        }
    }
}
