using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Timers;

namespace VTZabbixMonitoring
{
    internal class timers
    {
        static void ReStartService(string serviceName)
        {
            ServiceController service = new ServiceController(serviceName);
            if (service.Status != ServiceControllerStatus.Stopped)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(10));
                if (service.Status != ServiceControllerStatus.StopPending)
                {
                    foreach (var process in Process.GetProcessesByName(serviceName))
                    {
                        process.Kill();
                        Logs.WriteLine($"********** Service {serviceName} KIILL **********");
                    }
                }
            }
            Logs.WriteLine($">>>> Service {serviceName} status >>>> {service.Status} <<<<");

            Thread.Sleep(5000);

            if (service.Status != ServiceControllerStatus.Running)
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(10));
            }
            Logs.WriteLine($">>>> Service {serviceName} status >>>> {service.Status} <<<<");
        }

        static void RebootHost()
        {
            Logs.WriteLine($"***** Reboot *****");
            var cmd = new ProcessStartInfo("shutdown.exe", "-r -t 0");
            cmd.CreateNoWindow = true;
            cmd.UseShellExecute = false;
            cmd.ErrorDialog = false;
            Process.Start(cmd);
        }

        static bool replicator = false;
        static bool violation = false;
        static bool export = false;

        public static void OnReplicatorStatus(Object source, ElapsedEventArgs e)
        {
            UInt32 seconds = sql.LastReplicationSeconds();

            if (!replicator && seconds > (Service.restartingNoViolationIntervalHours * 3600))
            {
                replicator = true;
                ReStartService("VTTrafficReplicator");
            }
            if (seconds > ((Service.restartingNoViolationIntervalHours * 3600) + 3600))
            {
                RebootHost();
            }
            Logs.WriteLine($"Replication delay in seconds {seconds}.");
        }

        public static void OnViolationStatus(Object source, ElapsedEventArgs e)
        {
            UInt32 count = sql.UnprocessedViolationsCount(); ;
            UInt32 seconds = sql.UnprocessedViolationsSeconds();

            if (!violation && seconds > (Service.restartingNoViolationIntervalHours * 3600))
            {
                violation = true;
                ReStartService("VTViolations");
            }
            if (seconds > ((Service.restartingNoViolationIntervalHours * 3600) + 3600))
            {
                RebootHost();
            }
            Logs.WriteLine($"The delay in processing results in seconds is {seconds}, in the amount of {count}.");
        }

        public static void OnExportStatus(Object source, ElapsedEventArgs e)
        {
            UInt32 count = sql.UnexportedCount(); ;
            UInt32 seconds = sql.UnexportedSeconds();

            if (!export && seconds > (Service.restartingNoExportIntervalHours * 3600))
            {
                export = true;
                ReStartService("VTTrafficExport");
            }
            if (seconds > ((Service.restartingNoExportIntervalHours * 3600) + 3600))
            {
                RebootHost();
            }
            Logs.WriteLine($"The export delay in seconds is {seconds}, in the amount of {count}.");
        }

    }
}
