using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;

namespace WmsServer
{
    #region Windows服务发布
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new WmsServer() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
    #endregion

    //#region 控制台调试
    //class Program
    //{
    //    static GetTcpListener listener = new GetTcpListener();
    //    static void Main(string[] args)
    //    {
    //        Hello();
    //    }

    //    public static void Hello()
    //    {
    //        listener.GetListener();
    //    }
    //}
    //#endregion
}

