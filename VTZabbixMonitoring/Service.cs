using System;
using Microsoft.Win32;
using System.Collections;
using System.Configuration;
using System.ServiceProcess;

namespace VTZabbixMonitoring
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }

        public static TimeSpan localZone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);

        public static Hashtable StatusJson = new Hashtable();

        public static string sourceFolderPr = "D:\\Duplo";
        public static string sourceFolderSc = "D:\\Doris";
        public static string sortingFolderPr = "D:\\!Duplo";
        public static string sortingFolderSc = "D:\\!Doris";

        public static bool sortingViolations = true;
        public static int storageDays = 30;
        public static int storageSortingIntervalMinutes = 20;
        public static bool storageXML = true;
        public static bool storageСollage = false;
        public static bool storageVideo = false;

        public static bool statusServicesReplicator = true;
        public static int statusServicesReplicatorIntervalMinutes = 5;
        public static int restartingNoReplicationIntervalHours = 1;

        public static bool statusServicesViolation = true;
        public static int statusServicesViolationIntervalMinutes = 5;
        public static int restartingNoViolationIntervalHours = 1;

        public static bool statusServicesExport = true;
        public static int statusServicesExportIntervalMinutes = 5;
        public static int restartingNoExportIntervalHours = 6;

        public static bool statusHost = true;
        public static int statusHostIntervalMinutes = 5;

        public static string networkInterfaceForMonitoring = "Microsoft Hyper-V Network Adapter";

        public static string DiskMonitoring = "D:\\";

        public static string sqlSource = "(LOCAL)";
        public static string sqlUser = "sa";
        public static string sqlPassword = "1";

        public static bool statusWeb = true;

        void LoadConfig()
        {
            Logs.WriteLine("------------------------- Monitoring Service Settings -------------------------");

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\VTMonitoring", true))
            {
                if (key.GetValue("FailureActions") == null)
                {
                    key.SetValue("FailureActions", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x60, 0xea, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x60, 0xea, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x60, 0xea, 0x00, 0x00 });
                }
            }

            if (ConfigurationManager.AppSettings.Count != 0)
            {
                sourceFolderPr = ConfigurationManager.AppSettings["SourceFolderPr"];
                sortingFolderPr = ConfigurationManager.AppSettings["SortingFolderPr"];

                sourceFolderSc = ConfigurationManager.AppSettings["SourceFolderSc"];
                sortingFolderSc = ConfigurationManager.AppSettings["SortingFolderSc"];

                sortingViolations = Convert.ToBoolean(ConfigurationManager.AppSettings["SortingViolations"]);
                storageDays = Convert.ToInt32(ConfigurationManager.AppSettings["StorageDays"]);
                storageSortingIntervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["StorageSortingIntervalMinutes"]);
                storageXML = Convert.ToBoolean(ConfigurationManager.AppSettings["StorageXML"]);
                storageСollage = Convert.ToBoolean(ConfigurationManager.AppSettings["StorageСollage"]);
                storageVideo = Convert.ToBoolean(ConfigurationManager.AppSettings["StorageVideo"]);

                statusServicesReplicator = Convert.ToBoolean(ConfigurationManager.AppSettings["StatusServicesReplicator"]);
                statusServicesReplicatorIntervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["StatusServicesReplicatorIntervalMinutes"]);
                restartingNoReplicationIntervalHours = Convert.ToInt32(ConfigurationManager.AppSettings["RestartingNoReplicationIntervalHours"]);

                statusServicesViolation = Convert.ToBoolean(ConfigurationManager.AppSettings["StatusServicesViolation"]);
                statusServicesViolationIntervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["StatusServicesViolationIntervalMinutes"]);
                restartingNoViolationIntervalHours = Convert.ToInt32(ConfigurationManager.AppSettings["RestartingNoViolationIntervalHours"]);

                statusServicesExport = Convert.ToBoolean(ConfigurationManager.AppSettings["StatusServicesExport"]);
                statusServicesExportIntervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["StatusServicesExportIntervalMinutes"]);
                restartingNoExportIntervalHours = Convert.ToInt32(ConfigurationManager.AppSettings["RestartingNoExportIntervalHours"]);

                statusHost = Convert.ToBoolean(ConfigurationManager.AppSettings["StatusHost"]);
                statusHostIntervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["StatusHostIntervalMinutes"]);

                networkInterfaceForMonitoring = ConfigurationManager.AppSettings["NetworkInterfaceForMonitoring"];

                sqlSource = ConfigurationManager.AppSettings["SQLDataSource"];
                sqlUser = ConfigurationManager.AppSettings["SQLUser"];
                sqlPassword = ConfigurationManager.AppSettings["SQLPassword"];
            }

            if (sortingViolations)
            {
                var storageTimer = new System.Timers.Timer(storageSortingIntervalMinutes * 60000);
                storageTimer.Elapsed += Sorting.OnStorageTimer;
                storageTimer.AutoReset = true;
                storageTimer.Enabled = true;
                Logs.WriteLine($">>>>> Violation sorting is enabled at {storageSortingIntervalMinutes} minute intervals.");
            }

            if (statusServicesReplicator)
            {
                var statusServicesReplicatorTimer = new System.Timers.Timer(statusServicesReplicatorIntervalMinutes * 60000);
                statusServicesReplicatorTimer.Elapsed += timers.OnReplicatorStatus;
                statusServicesReplicatorTimer.AutoReset = true;
                statusServicesReplicatorTimer.Enabled = true;
                Logs.WriteLine($">>>>> Replication service monitoring is enabled at {statusServicesReplicatorIntervalMinutes} minute intervals.");
            }

            if (statusServicesViolation)
            {
                var statusServicesViolationTimer = new System.Timers.Timer(statusServicesViolationIntervalMinutes * 60000);
                statusServicesViolationTimer.Elapsed += timers.OnViolationStatus;
                statusServicesViolationTimer.AutoReset = true;
                statusServicesViolationTimer.Enabled = true;
                Logs.WriteLine($">>>>> Violation service monitoring is enabled at {statusServicesViolationIntervalMinutes} minute intervals.");
            }

            if (statusServicesExport)
            {
                var statusServicesExportTimer = new System.Timers.Timer(statusServicesExportIntervalMinutes * 60000);
                statusServicesExportTimer.Elapsed += timers.OnExportStatus;
                statusServicesExportTimer.AutoReset = true;
                statusServicesExportTimer.Enabled = true;
                Logs.WriteLine($">>>>> Export service monitoring is enabled at {statusServicesExportIntervalMinutes} minute intervals.");
            }

            if (statusHost)
            {
                var statusHostTimer = new System.Timers.Timer(statusHostIntervalMinutes * 60000);
                statusHostTimer.Elapsed += timers.OnHostStatus;
                statusHostTimer.AutoReset = true;
                statusHostTimer.Enabled = true;
                Logs.WriteLine($">>>>> Host monitoring is enabled at {statusHostIntervalMinutes} minute intervals.");
            }

            Logs.WriteLine("-------------------------------------------------------------------------------");

        }

        void CreatedStatusJson()
        {
            StatusJson.Add("UpTime", timers.GetUpTime().ToString());

            StatusJson.Add("DiskTotalSize", (timers.GetDiskTotalSize() / 1_073_741_824).ToString());
            StatusJson.Add("DiskTotalFreeSpace", (timers.GetDiskTotalFreeSpace() / 1_073_741_824).ToString());
            StatusJson.Add("DiskPercentTotalSize", timers.GetDiskUsagePercentage().ToString());
            StatusJson.Add("DiskPercentTotalFreeSpace", timers.GetDiskPercentFreeSpace().ToString());

            StatusJson.Add("NetworkSent", timers.GetNetworkSent().ToString());
            StatusJson.Add("NetworkReceived", timers.GetNetworkReceived().ToString());

            StatusJson.Add("ArchiveDepthSeconds", sql.ArchiveDepthSeconds());
            StatusJson.Add("ArchiveDepthCount", sql.ArchiveDepthCount());

            StatusJson.Add("LastReplicationSeconds", sql.LastReplicationSeconds().ToString());
            StatusJson.Add("UnprocessedViolationsCount", sql.UnprocessedViolationsCount().ToString());
            StatusJson.Add("UnprocessedViolationsSeconds", sql.UnprocessedViolationsSeconds().ToString());
            StatusJson.Add("UnexportedCount", sql.UnexportedCount().ToString());
            StatusJson.Add("UnexportedSeconds", sql.UnexportedSeconds().ToString());
        }

        protected override void OnStart(string[] args)
        {
            Logs.WriteLine("*******************************************************************************");
            Logs.WriteLine("************************** Service Monitoring START ***************************");
            Logs.WriteLine("*******************************************************************************");
            LoadConfig();
            CreatedStatusJson();
            Sorting.HashVuolation();
            web.WEBServer.Start();
        }

        protected override void OnStop()
        {
            statusWeb = false;
            web.WEBServer.Interrupt();
            Logs.WriteLine("*******************************************************************************");
            Logs.WriteLine("*************************** Service Monitoring STOP ***************************");
            Logs.WriteLine("*******************************************************************************");
        }
    }
}
