using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace ServiceSetup
{
    public class LogAPI
    {
        private static string myPath = "";
        private static string myName = "";

        /// <summary>  
        /// 初始化日志文件  
        /// </summary>  
        /// <param name="logPath"></param>  
        /// <param name="logName"></param>  
        public static void InitLogAPI(string logPath, string logName)
        {
            myPath = logPath;
            myName = logName;
        }

        /// <summary>  
        /// 写入日志  
        /// </summary>  
        /// <param name="ex">日志信息</param>  
        public static void WriteLog(string ex)
        {
            if (myPath == "" || myName == "")
                return;

            string Year = DateTime.Now.Year.ToString();
            string Month = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string Day = DateTime.Now.Day.ToString().PadLeft(2, '0');

            //年月日文件夹是否存在，不存在则建立  
            if (!Directory.Exists(myPath + "\\LogFiles\\" + Year + "_" + Month + "\\" + Year + "_" + Month + "_" + Day))
            {
                Directory.CreateDirectory(myPath + "\\LogFiles\\" + Year + "_" + Month + "\\" + Year + "_" + Month + "_" + Day);
            }

            //写入日志UNDO,Exception has not been handle  
            string LogFile = myPath + "\\LogFiles\\" + Year + "_" + Month + "\\" + Year + "_" + Month + "_" + Day + "\\" + myName;
            if (!File.Exists(LogFile))
            {
                System.IO.StreamWriter myFile;
                myFile = System.IO.File.AppendText(LogFile);
                myFile.Close();
            }

            while (true)
            {
                try
                {
                    StreamWriter sr = File.AppendText(LogFile);
                    sr.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  " + ex);
                    sr.Close();
                    break;
                }
                catch (Exception e)
                {
                    System.Threading.Thread.Sleep(50);
                    continue;
                }
            }

        }

    }
}