using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace RApID_Service
{
    public partial class RapidService : ServiceBase
    {
        private int eventId = 1;

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        public RapidService()
        {
            InitializeComponent();

            eventLogger = new EventLog();
            if(!EventLog.SourceExists("RApIDSource"))
            {
                EventLog.CreateEventSource("RApIDSource", "MainLog");
            }
            eventLogger.Source = "RApIDSource";
            eventLogger.Log = "MainLog";
        }

        protected override void OnStart(string[] args)
        {
            eventLogger.WriteEntry("START: RApID Service");

            var timer = new System.Timers.Timer { Interval = 10000 };
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();

            var status = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_START_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(ServiceHandle, ref status);
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            //TODO: Insert monitoring things here.
            eventLogger.WriteEntry("RApID: Monitoring started...", EventLogEntryType.Information, eventId++);
        }

        protected override void OnPause()
        {
            eventLogger.WriteEntry("PAUSED: RApID Service");

            var status = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_PAUSE_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(ServiceHandle, ref status);
        }

        protected override void OnContinue()
        {
            eventLogger.WriteEntry("RESUMED: RApID Service");

            var status = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_CONTINUE_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(ServiceHandle, ref status);
        }

        protected override void OnStop()
        {
            eventLogger.WriteEntry("STOP: RApID Service");

            var status = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_STOP_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(ServiceHandle, ref status);
        }

        protected override void OnShutdown()
        {
            eventLogger.WriteEntry("SHUTDOWN: RApID Service");
        }
    }
}
