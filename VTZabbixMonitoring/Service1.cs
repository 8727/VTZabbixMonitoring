using System;
using System.ServiceProcess;

namespace VTZabbixMonitoring
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logs.WriteLine("*******************************************************************************");
            Logs.WriteLine("************************** Service Monitoring START ***************************");
            Logs.WriteLine("*******************************************************************************");
            //LoadConfig();
            //Sorting.HashVuolation();
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
