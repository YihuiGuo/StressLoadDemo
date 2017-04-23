using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StressLoadDemo.Model.DataProvider
{
    public class HubDebugReceiver 
    {
        public double totalDevice { get ; set ; }
        public double totalMessage { get ; set ; }
        public double partitionNumber { get; set; }

        private Thread _hubDeviceThread, _hubMsgThread;

        public void PauseReceive()
        {
            _hubDeviceThread = new Thread(() => { });
            _hubMsgThread = new Thread(() => { });
            _hubDeviceThread.Start();
            _hubMsgThread.Start();
        }
        
        public void StartReceive()
        {
            _hubDeviceThread = new Thread(() => Calculatesin());
            _hubMsgThread = new Thread(() => Calculateln());
            _hubDeviceThread.Start();
            _hubMsgThread.Start();
        }
        void Calculatesin()
        {
            while (true)
            {
                for (double i = -10; ; i += 0.1)
                {
                    totalDevice = 20 * Math.Sin(i);
                    Thread.Sleep(100);
                }
            }
        }

        void Calculateln()
        {
            while (true)
            {
                for (double i = 0; ; i += 0.1)
                {
                    totalMessage = Math.Tan(i);
                    Thread.Sleep(100);
                }
            }
        }
    }
}
