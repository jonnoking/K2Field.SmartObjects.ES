using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace K2Field.SmartObjects.ES.Service
{
    partial class K2CEPListenerService : ServiceBase
    {

        private System.ComponentModel.IContainer components;
        private System.Diagnostics.EventLog eventLog1;

        public K2CEPListenerService()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("K2CEPListener"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "K2CEPListener", "K2CEPListenerLog");
            }
            eventLog1.Source = "K2CEPListener";
            eventLog1.Log = "K2CEPListenerLog";

            this.AutoLog = false;

        }


        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Started...");



        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
}
