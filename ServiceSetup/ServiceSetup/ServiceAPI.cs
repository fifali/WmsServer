using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
//using System.Threading.Tasks;
using Microsoft.Win32;

namespace ServiceSetup
{
    public class ServiceAPI
    {
        /// <summary>  
        /// 检查服务存在的存在性  
        /// </summary>  
        /// <param name=" NameService ">服务名</param>  
        /// <returns>存在返回 true,否则返回 false;</returns>  
        public static bool isServiceIsExisted(string NameService)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services)
            {
                if (s.ServiceName.ToLower() == NameService.ToLower())
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>  
        /// 安装Windows服务  
        /// </summary>  
        /// <param name="stateSaver">集合</param>  
        /// <param name="filepath">程序文件路径</param>  
        public static void InstallmyService(IDictionary stateSaver, string filepath)
        {
            AssemblyInstaller AssemblyInstaller1 = new AssemblyInstaller();
            AssemblyInstaller1.UseNewContext = true;
            AssemblyInstaller1.Path = filepath;
            AssemblyInstaller1.Install(stateSaver);
            AssemblyInstaller1.Commit(stateSaver);
            AssemblyInstaller1.Dispose();
        }
        /// <summary>  
        /// 卸载Windows服务  
        /// </summary>  
        /// <param name="filepath">程序文件路径</param>  
        public static void UnInstallmyService(string filepath)
        {
            AssemblyInstaller AssemblyInstaller1 = new AssemblyInstaller();
            AssemblyInstaller1.UseNewContext = true;
            AssemblyInstaller1.Path = filepath;
            AssemblyInstaller1.Uninstall(null);
            AssemblyInstaller1.Dispose();
        }

        /// <summary>  
        /// 启动服务  
        /// </summary>  
        /// <param name=" NameService ">服务名</param>  
        /// <returns>存在返回 true,否则返回 false;</returns>  
        public static bool RunService(string NameService)
        {
            bool bo = true;
            try
            {
                ServiceController sc = new ServiceController(NameService);
                if (sc.Status.Equals(ServiceControllerStatus.Stopped) || sc.Status.Equals(ServiceControllerStatus.StopPending))
                {
                    sc.Start();
                }
            }
            catch (Exception ex)
            {
                bo = false;
                LogAPI.WriteLog(ex.Message);
            }

            return bo;
        }

        /// <summary>  
        /// 停止服务  
        /// </summary>  
        /// <param name=" NameService ">服务名</param>  
        /// <returns>存在返回 true,否则返回 false;</returns>  
        public static bool StopService(string NameService)
        {
            bool bo = true;
            try
            {
                ServiceController sc = new ServiceController(NameService);
                if (!sc.Status.Equals(ServiceControllerStatus.Stopped))
                {
                    sc.Stop();
                }
            }
            catch (Exception ex)
            {
                bo = false;
                LogAPI.WriteLog(ex.Message);
            }

            return bo;
        }

        /// <summary>  
        /// 获取服务状态  
        /// </summary>  
        /// <param name=" NameService ">服务名</param>  
        /// <returns>返回服务状态</returns>  
        public static int GetServiceStatus(string NameService)
        {
            int ret = 0;
            try
            {
                ServiceController sc = new ServiceController(NameService);
                ret = Convert.ToInt16(sc.Status);
            }
            catch (Exception ex)
            {
                ret = 0;
                LogAPI.WriteLog(ex.Message);
            }

            return ret;
        }

        /// <summary>  
        /// 获取服务安装路径  
        /// </summary>  
        /// <param name="ServiceName"></param>  
        /// <returns></returns>  
        public static string GetWindowsServiceInstallPath(string ServiceName)
        {
            string path = "";
            try
            {
                string key = @"SYSTEM\CurrentControlSet\Services\" + ServiceName;
                path = Registry.LocalMachine.OpenSubKey(key).GetValue("ImagePath").ToString();

                path = path.Replace("\"", string.Empty);//替换掉双引号    

                FileInfo fi = new FileInfo(path);
                path = fi.Directory.ToString();
            }
            catch (Exception ex)
            {
                path = "";
                LogAPI.WriteLog(ex.Message);
            }
            return path;
        }

        /// <summary>  
        /// 获取指定服务的版本号  
        /// </summary>  
        /// <param name="serviceName">服务名称</param>  
        /// <returns></returns>  
        public static string GetServiceVersion(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                return string.Empty;
            }
            try
            {
                string path = GetWindowsServiceInstallPath(serviceName) + "\\" + serviceName + ".exe";
                Assembly assembly = Assembly.LoadFile(path);
                AssemblyName assemblyName = assembly.GetName();
                Version version = assemblyName.Version;
                return version.ToString();
            }
            catch (Exception ex)
            {
                LogAPI.WriteLog(ex.Message);
                return string.Empty;
            }
        }
    }
}