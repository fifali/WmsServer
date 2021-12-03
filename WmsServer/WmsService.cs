using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

namespace WmsServer
{
    public partial class WmsServer : ServiceBase
    {
        private GetTcpListener listener = new GetTcpListener();
        public bool IsRunning = false;
        System.Timers.Timer t = null;
        public WmsServer()
        {
            InitializeComponent();           
        }

        public void TimeElapse(object source, System.Timers.ElapsedEventArgs e)
        {
            if (!IsRunning)
            {
                t.Stop();
                IsRunning = true;
                listener.GetListener();
            }
        }    

        protected override void OnStart(string[] args)
        {
            if (!IsRunning)
            {
                t = new System.Timers.Timer(5000);//实例化Timer类，设置间隔时间为10000毫秒；      
                t.Elapsed += new System.Timers.ElapsedEventHandler(TimeElapse);//到达时间的时候执行事件；      
                t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；      
                t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            }
        }

        protected override void OnStop()
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }
    }
}

