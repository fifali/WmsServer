using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ServiceSetup
{
    public partial class FrmServiceSetup : Form
    {
        string strServiceName = string.Empty;
        public FrmServiceSetup()
        {
            InitializeComponent();
            strServiceName = string.IsNullOrEmpty(lblServiceName.Text) ? "Wechat" : lblServiceName.Text;  
            InitControlStatus(strServiceName, btnInstallOrUninstall, btnStartOrEnd, btnGetStatus, lblMsg, gbMain);
        }

        /// <summary>  
        /// 初始化控件状态  
        /// </summary>  
        /// <param name="serviceName">服务名称</param>  
        /// <param name="btn1">安装按钮</param>  
        /// <param name="btn2">启动按钮</param>  
        /// <param name="btn3">获取状态按钮</param>  
        /// <param name="txt">提示信息</param>  
        /// <param name="gb">服务所在组合框</param>  
        void InitControlStatus(string serviceName, Button btn1, Button btn2, Button btn3, Label txt, GroupBox gb)
        {
            try
            {
                btn1.Enabled = true;

                if (ServiceAPI.isServiceIsExisted(serviceName))
                {
                    btn3.Enabled = true;
                    btn2.Enabled = true;
                    btn1.Text = "卸载服务";
                    int status = ServiceAPI.GetServiceStatus(serviceName);
                    if (status == 4)
                    {
                        btn2.Text = "停止服务";
                    }
                    else
                    {
                        btn2.Text = "启动服务";
                    }
                    GetServiceStatus(serviceName, txt);
                    //获取服务版本  
                    string temp = string.IsNullOrEmpty(ServiceAPI.GetServiceVersion(serviceName)) ? string.Empty : "(" + ServiceAPI.GetServiceVersion(serviceName) + ")";
                    gb.Text += temp;
                }
                else
                {
                    btn1.Text = "安装服务";
                    btn2.Enabled = false;
                    btn3.Enabled = false;
                    txt.Text = "服务【" + serviceName + "】未安装！";
                }
            }
            catch (Exception ex)
            {
                txt.Text = "error";
                LogAPI.WriteLog(ex.Message);
            }
        }

        /// <summary>  
        /// 安装或卸载服务  
        /// </summary>  
        /// <param name="serviceName">服务名称</param>  
        /// <param name="btnSet">安装、卸载</param>  
        /// <param name="btnOn">启动、停止</param>  
        /// <param name="txtMsg">提示信息</param>  
        /// <param name="gb">组合框</param>  
        void SetServerce(string serviceName, Button btnSet, Button btnOn, Button btnShow, Label txtMsg, GroupBox gb)
        {
            try
            {
                string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string serviceFileName = location.Substring(0, location.LastIndexOf('\\')) + "\\" + serviceName + ".exe";

                if (btnSet.Text == "安装服务")
                {
                    ServiceAPI.InstallmyService(null, serviceFileName);
                    if (ServiceAPI.isServiceIsExisted(serviceName))
                    {
                        txtMsg.Text = "服务【" + serviceName + "】安装成功！";
                        btnOn.Enabled = btnShow.Enabled = true;
                        string temp = string.IsNullOrEmpty(ServiceAPI.GetServiceVersion(serviceName)) ? string.Empty : "(" + ServiceAPI.GetServiceVersion(serviceName) + ")";
                        gb.Text += temp;
                        btnSet.Text = "卸载服务";
                        btnOn.Text = "启动服务";
                    }
                    else
                    {
                        txtMsg.Text = "服务【" + serviceName + "】安装失败，请检查日志！";
                    }
                }
                else
                {
                    ServiceAPI.UnInstallmyService(serviceFileName);
                    if (!ServiceAPI.isServiceIsExisted(serviceName))
                    {
                        txtMsg.Text = "服务【" + serviceName + "】卸载成功！";
                        btnOn.Enabled = btnShow.Enabled = false;
                        btnSet.Text = "安装服务";
                        //gb.Text =strServiceName;  
                    }
                    else
                    {
                        txtMsg.Text = "服务【" + serviceName + "】卸载失败，请检查日志！";
                    }
                }
            }
            catch (Exception ex)
            {
                txtMsg.Text = "error";
                LogAPI.WriteLog(ex.Message);
            }
        }

        //获取服务状态  
        void GetServiceStatus(string serviceName, Label txtStatus)
        {
            try
            {
                if (ServiceAPI.isServiceIsExisted(serviceName))
                {
                    string statusStr = "";
                    int status = ServiceAPI.GetServiceStatus(serviceName);
                    switch (status)
                    {
                        case 1:
                            statusStr = "服务未运行！";
                            break;
                        case 2:
                            statusStr = "服务正在启动！";
                            break;
                        case 3:
                            statusStr = "服务正在停止！";
                            break;
                        case 4:
                            statusStr = "服务正在运行！";
                            break;
                        case 5:
                            statusStr = "服务即将继续！";
                            break;
                        case 6:
                            statusStr = "服务即将暂停！";
                            break;
                        case 7:
                            statusStr = "服务已暂停！";
                            break;
                        default:
                            statusStr = "未知状态！";
                            break;
                    }
                    txtStatus.Text = statusStr;
                }
                else
                {
                    txtStatus.Text = "服务【" + serviceName + "】未安装！";
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = "error";
                LogAPI.WriteLog(ex.Message);
            }
        }

        //启动服务  
        void OnService(string serviceName, Button btn, Label txt)
        {
            try
            {
                if (btn.Text == "启动服务")
                {
                    ServiceAPI.RunService(serviceName);

                    int status = ServiceAPI.GetServiceStatus(serviceName);
                    if (status == 2 || status == 4 || status == 5)
                    {
                        txt.Text = "服务【" + serviceName + "】启动成功！";
                        btn.Text = "停止服务";
                    }
                    else
                    {
                        txt.Text = "服务【" + serviceName + "】启动失败！";
                    }
                }
                else
                {
                    ServiceAPI.StopService(serviceName);

                    int status = ServiceAPI.GetServiceStatus(serviceName);
                    if (status == 1 || status == 3 || status == 6 || status == 7)
                    {
                        txt.Text = "服务【" + serviceName + "】停止成功！";
                        btn.Text = "启动服务";
                    }
                    else
                    {
                        txt.Text = "服务【" + serviceName + "】停止失败！";
                    }
                }
            }
            catch (Exception ex)
            {
                txt.Text = "error";
                LogAPI.WriteLog(ex.Message);
            }
        }

        //安装or卸载服务  
        private void btnInstallOrUninstall_Click(object sender, EventArgs e)
        {
            btnInstallOrUninstall.Enabled = false;
            SetServerce(strServiceName, btnInstallOrUninstall, btnStartOrEnd, btnGetStatus, lblMsg, gbMain);
            btnInstallOrUninstall.Enabled = true;
            btnInstallOrUninstall.Focus();
        }

        //启动or停止服务  
        private void btnStartOrEnd_Click(object sender, EventArgs e)
        {
            btnStartOrEnd.Enabled = false;
            OnService(strServiceName, btnStartOrEnd, lblMsg);
            btnStartOrEnd.Enabled = true;
            btnStartOrEnd.Focus();
        }

        //获取服务状态  
        private void btnGetStatus_Click(object sender, EventArgs e)
        {
            btnGetStatus.Enabled = false;
            GetServiceStatus(strServiceName, lblMsg);
            btnGetStatus.Enabled = true;
            btnGetStatus.Focus();
        }

        private void FrmServiceSetup_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)    //最小化到系统托盘  
            {
                notifyIcon1.Visible = true;    //显示托盘图标  
                this.ShowInTaskbar = false;
                this.Hide();    //隐藏窗口  
            }
        }

        private void FrmServiceSetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("是缩小到托盘?", "确认", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                // 取消关闭窗体  
                e.Cancel = true;
                // 将窗体变为最小化  
                this.WindowState = FormWindowState.Minimized;
            }
            else if (result == DialogResult.No)
            {
                System.Environment.Exit(0);
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true; //显示在系统任务栏   
                //notifyIcon1.Visible = false; //托盘图标不可见   
                this.Activate();
            }
        }

        private void 打开主界面ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        //private void 打开主界面ToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    this.Show();
        //    this.WindowState = FormWindowState.Normal;
        //    this.ShowInTaskbar = true; //显示在系统任务栏   
        //    notifyIcon1.Visible = false; //托盘图标不可见   
        //    this.Activate();
        //}

     

        //private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    System.Environment.Exit(0);
        //    ExitProcess();
        //}
        ////结束进程  
        //private void ExitProcess()
        //{
        //    System.Environment.Exit(0);
        //    Process[] ps = Process.GetProcesses();
        //    foreach (Process item in ps)
        //    {
        //        if (item.ProcessName == "ServiceSetup")
        //        {
        //            item.Kill();
        //        }
        //    }
        //}
    }
}  
