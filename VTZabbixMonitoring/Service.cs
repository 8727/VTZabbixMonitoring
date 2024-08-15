using Microsoft.Win32;
using System;
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


        public static string sqlSource = "(LOCAL)";
        public static string sqlUser = "sa";
        public static string sqlPassword = "1";

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

                //statusViewCamera = Convert.ToBoolean(ConfigurationManager.AppSettings["StatusViewCamera"]);
                //statusViewCameraIntervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["StatusViewCameraIntervalMinutes"]);

                //statusServicesReplicator = Convert.ToBoolean(ConfigurationManager.AppSettings["StatusServicesReplicator"]);
                //statusServicesReplicatorIntervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["StatusServicesReplicatorIntervalMinutes"]);
                //restartingNoReplicationIntervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["RestartingNoReplicationIntervalMinutes"]);

                //statusServicesExport = Convert.ToBoolean(ConfigurationManager.AppSettings["StatusServicesExport"]);
                //statusServicesExportIntervalHours = Convert.ToInt32(ConfigurationManager.AppSettings["StatusServicesExportIntervalHours"]);

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
                Logs.WriteLine($">>>>> Violation sorting is enabled at {storageSortingIntervalMinutes} minute intervals");
            }

        }

        protected override void OnStart(string[] args)
        {
            Logs.WriteLine("*******************************************************************************");
            Logs.WriteLine("************************** Service Monitoring START ***************************");
            Logs.WriteLine("*******************************************************************************");
            LoadConfig();
            Sorting.HashVuolation();
            //web.WEBServer.Start();
        }

        protected override void OnStop()
        {
            //web.statusWeb = false;
            //web.WEBServer.Interrupt();
            Logs.WriteLine("*******************************************************************************");
            Logs.WriteLine("*************************** Service Monitoring STOP ***************************");
            Logs.WriteLine("*******************************************************************************");
        }
    }
}
