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
            
            Logs.WriteLine($">>>>> Replication service monitoring is enabled.");
        }

        public static void OnViolationStatus(Object source, ElapsedEventArgs e)
        {
            if (!violation && sql.SqlUnprocessedViolationsSecondes() > (Service.restartingNoViolationIntervalHours * 3600))
            {
                violation = true;
                ReStartService("VTTrafficReplicator");
            }
            if (sql.SqlUnprocessedViolationsSecondes() > ((Service.restartingNoViolationIntervalHours * 3600) + 3600))
            {
                RebootHost();
            }

            Logs.WriteLine($">>>>> Violation service monitoring is enabled.");
        }

        public static void OnExportStatus(Object source, ElapsedEventArgs e)
        {

            Logs.WriteLine($">>>>> Export service monitoring is enabled.");
        }

    }
}
