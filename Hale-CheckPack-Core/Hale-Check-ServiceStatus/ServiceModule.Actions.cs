using HaleLib.Modules;
using HaleLib.Modules.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Hale.Modules
{
    partial class ServiceModule
    {
        public ActionFunctionResult StartServiceAction(ActionSettings settings)
        {
            var afr = new ActionFunctionResult();

            var globalTimeout = settings.ContainsKey("timeout") ?
                TimeSpan.Parse(settings["timeout"]) :
                TimeSpan.FromSeconds(6);

            foreach (var kvpTarget in settings.TargetSettings)
            {
                var result = _serviceControlAction(settings, kvpTarget.Value, kvpTarget.Key, ServiceControllerStatus.Running, globalTimeout);
                afr.ActionResults.Add(kvpTarget.Key, result);
            }

            afr.RanSuccessfully = true;

            return afr;
        }

        public ActionFunctionResult StopServiceAction(ActionSettings settings)
        {
            var afr = new ActionFunctionResult();

            var globalTimeout = settings.ContainsKey("timeout") ?
                TimeSpan.Parse(settings["timeout"]) :
                TimeSpan.FromSeconds(6);

            foreach (var kvpTarget in settings.TargetSettings)
            {
                var result = _serviceControlAction(settings, kvpTarget.Value, kvpTarget.Key, ServiceControllerStatus.Stopped, globalTimeout);
                afr.ActionResults.Add(kvpTarget.Key, result);
            }

            afr.RanSuccessfully = true;

            return afr;
        }

        public ActionFunctionResult RestartServiceAction(ActionSettings settings)
        {
            var afr = new ActionFunctionResult();

            var globalTimeout = settings.ContainsKey("timeout") ?
                TimeSpan.Parse(settings["timeout"]) :
                TimeSpan.FromSeconds(6);

            foreach (var kvpTarget in settings.TargetSettings)
            {
                var start = DateTime.Now;
                var resultStop = _serviceControlAction(settings, kvpTarget.Value, kvpTarget.Key, ServiceControllerStatus.Stopped, globalTimeout);
                if (resultStop.RanSuccessfully)
                {
                    var resultStart = _serviceControlAction(settings, kvpTarget.Value, kvpTarget.Key, ServiceControllerStatus.Running, globalTimeout);
                    if (resultStart.RanSuccessfully)
                    {
                        afr.ActionResults.Add(kvpTarget.Key, new ActionResult()
                        {
                            RanSuccessfully = true,
                            Message = $"Service {kvpTarget.Key} successfully restarted."
                        });
                    }
                    else
                    {
                        afr.ActionResults.Add(kvpTarget.Key, new ActionResult()
                        {
                            RanSuccessfully = false,
                            Message = resultStart.Message,
                            ExecutionException = resultStart.ExecutionException
                        });
                    }
                }
                else
                {
                    afr.ActionResults.Add(kvpTarget.Key, new ActionResult()
                    {
                        RanSuccessfully = false,
                        Message = resultStop.Message,
                        ExecutionException = resultStop.ExecutionException
                    });
                }

            }

            afr.RanSuccessfully = true;

            return afr;
        }

        ActionResult _serviceControlAction(ActionSettings settings, Dictionary<string, string> targetSettings,
            string target, ServiceControllerStatus newStatus, TimeSpan globalTimeout)
        {
            var result = new ActionResult();
            try
            {
                var timeout = settings.ContainsKey("timeout") ?
                        TimeSpan.Parse(settings["timeout"]) :
                        globalTimeout;
                var sc = targetSettings.ContainsKey("machine") ?
                    new ServiceController(target, targetSettings["machine"]) :
                    new ServiceController(target);
                var start = DateTime.Now;
                if(newStatus == ServiceControllerStatus.Running)
                    sc.Start();
                if (newStatus == ServiceControllerStatus.Stopped)
                    sc.Stop();
                sc.WaitForStatus(newStatus, timeout);
                var elapsed = DateTime.Now - start;
                if (sc.Status == newStatus)
                {
                    result.RanSuccessfully = true;
                    if(newStatus == ServiceControllerStatus.Running)
                        result.Message = $"Service {target} was started successfully in {elapsed.TotalSeconds:f1} seconds.";
                    if (newStatus == ServiceControllerStatus.Stopped)
                        result.Message = $"Service {target} was stopped successfully in {elapsed.TotalSeconds:f1} seconds.";
                }
                else
                {
                    result.RanSuccessfully = false;
                    if (newStatus == ServiceControllerStatus.Running)
                        result.Message = $"Service {target} failed to start, current status is {sc.Status.ToString()}.";
                    if (newStatus == ServiceControllerStatus.Stopped)
                        result.Message = $"Service {target} failed to stop, current status is {sc.Status.ToString()}.";
                    result.ExecutionException = new ServiceCommandException("Timed out while waiting for service status.");
                }
            }
            catch (Exception x)
            {
                result.ExecutionException = x;
                result.Message = x.Message;
                result.RanSuccessfully = false;
            }
            return result;

        }

        class ServiceCommandException: Exception
        {
            public ServiceCommandException() : base() { }
            public ServiceCommandException(string m): base(m) { }
            public ServiceCommandException(string m, Exception x) : base(m, x) { }
        }
    }
}
