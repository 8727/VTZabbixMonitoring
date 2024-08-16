using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Timers;

namespace VTZabbixMonitoring
{
    internal class timers
    {
        static bool replicator = false;
        static bool violation = false;
        static bool export = false;

        static DriveInfo driveInfo = new DriveInfo(Service.DiskMonitoring);

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

        public static UInt32 GetUpTime()
        {
            PerformanceCounter uptime = new PerformanceCounter("System", "System Up Time");
            uptime.NextValue();
            return Convert.ToUInt32(uptime.NextValue());
        }

        public static long GetDiskTotalSize()
        {
            return driveInfo.TotalSize;
        }

        public static long GetDiskTotalFreeSpace()
        {
            return driveInfo.TotalFreeSpace; ;
        }

        public static byte GetDiskUsagePercentage()
        {
            return Convert.ToByte(driveInfo.TotalFreeSpace / (driveInfo.TotalSize / 100));
        }

        public static byte GetDiskPercentFreeSpace()
        {
            return Convert.ToByte(100 - (driveInfo.TotalFreeSpace / (driveInfo.TotalSize / 100)));
        }

        public static UInt16 GetNetworkSent()
        {
            PerformanceCounter counterSent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", Service.networkInterfaceForMonitoring);
            counterSent.NextValue();
            return Convert.ToUInt16(counterSent.NextValue() * 8 / 1024);
        }

        public static UInt16 GetNetworkReceived()
        {
            PerformanceCounter counterReceived = new PerformanceCounter("Network Interface", "Bytes Received/sec", Service.networkInterfaceForMonitoring);
            counterReceived.NextValue();
            return Convert.ToUInt16(counterReceived.NextValue() * 8 / 1024);
        }

        public static void OnReplicatorStatus(Object source, ElapsedEventArgs e)
        {
            UInt32 seconds = sql.LastReplicationSeconds();
            Service.StatusJson["LastReplicationSeconds"] = seconds.ToString();

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
            UInt32 count = sql.UnprocessedViolationsCount();
            UInt32 seconds = sql.UnprocessedViolationsSeconds();

            Service.StatusJson["UnprocessedViolationsCount"] = count.ToString();
            Service.StatusJson["UnprocessedViolationsSeconds"] = seconds.ToString();

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
            UInt32 count = sql.UnexportedCount();
            UInt32 seconds = sql.UnexportedSeconds();

            Service.StatusJson["UnexportedCount"] = count.ToString();
            Service.StatusJson["UnexportedSeconds"] = seconds.ToString();

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

        public static void OnHostStatus(Object source, ElapsedEventArgs e)
        {
            UInt32 upTimeUInt32 =  GetUpTime();
            Service.StatusJson["UpTime"] = upTimeUInt32.ToString();
            Logs.WriteLine($"Host uptime in seconds {upTimeUInt32}.");

            long diskSize = timers.GetDiskTotalSize() / 1_073_741_824;
            long diskFreeSpace = timers.GetDiskTotalFreeSpace() / 1_073_741_824;
            byte diskPercentSize = timers.GetDiskUsagePercentage();
            byte diskPercentFreeSpace = timers.GetDiskPercentFreeSpace();

            Service.StatusJson["DiskTotalSize"] = diskSize.ToString();
            Service.StatusJson["DiskTotalFreeSpace"] = diskFreeSpace.ToString();
            Service.StatusJson["DiskPercentSize"] = diskPercentSize.ToString();
            Service.StatusJson["DiskPercentFreeSpace"] = diskPercentFreeSpace.ToString();
            Logs.WriteLine($"Total disk size {diskSize}, free space size {diskFreeSpace}, disk size as a percentage {diskPercentSize}, free disk space percentage {diskPercentFreeSpace}.");

            UInt16 networkSent = GetNetworkSent();
            UInt16 networkReceived = GetNetworkReceived();

            Logs.WriteLine($"Interface loading incoming {networkReceived}, outgoing {networkSent}.");
        }
    }
}
