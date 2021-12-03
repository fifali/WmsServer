//============================================================================
// 服务器服务程序: 采用异步线程通讯
//----------------------------------------------------------------------------
// 描述: 
//----------------------------------------------------------------------------
// 参数:(无)
//----------------------------------------------------------------------------
// 返回值:  (none)
//----------------------------------------------------------------------------
// 作者:	lwb		日期: 2015.09.22
//----------------------------------------------------------------------------
// 修改历史: 
//	
//============================================================================
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Data;
using System.Xml;
using System.IO;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using Oracle.ManagedDataAccess.Client;

namespace WmsServer
{
    public class GetTcpListener
    {
        #region 变量
        public MemoryStream memory = new MemoryStream();
        public XmlDocument xmlDoc = new XmlDocument();
        public XmlDocument doc = new XmlDocument();
        public OracleTransaction trans;                  /* 事务处理类 */
        public bool inTransaction = false;  /* 指示当前是否正处于事务中 */
        public OracleConnection cn;                     /* 数据库连接 */
        OracleCommand cmd = null;
        public string ls_curip;
        public string ipaddress;//Socket服务器IP地址
        public string webserviceip;//webservice接口地址
        public string ls_interfaceflag;//是否记录接口日志
        public int host;//Socket服务器端口
        public int webservicehost;
        public int timers;
        public int bytes = 1024;
        public TcpListener listener = null;
        public long i = 0;
        private bool done = false;
        public DataTable dt = null;
        public ArrayList lt = new ArrayList();
        public ArrayList sc = new ArrayList();
        control ct = new control();
        connectobject co = new connectobject();
        public ManualResetEvent allDone = new ManualResetEvent(false);
        #endregion

        #region 启动新线程监听
        public void GetListener()
        {
            #region 每个客户端请求都在客户端新建一个线程(非异步多线程处理)
            //Thread accept = new Thread(new ThreadStart(AcceptClientInfo));
            //accept.IsBackground = false;//后台线程
            //accept.Start();
            //TcpClient client = new TcpClient(ipaddress, 10001);
            #endregion
            #region 异步处理
            StartListening();
            #endregion
        }
        #endregion

        public static byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        public static string ToHexString(byte[] bytes)
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            }
            return hexString;
        }

        #region 非异步线程
        /// <summary>
        /// 线程执行函数
        /// TCPServer类的HandleConnection函数
        /// </summary>
        private void AcceptClientInfo()
        {
            try
            {
                int port;
                port = host;
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                while (!done)
                {
                    try
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        TCPServer server = new TCPServer();
                        server.client = client;
                        server.listener = listener;
                        Thread clientThread = new Thread(new ThreadStart(server.HandleConnection));
                        clientThread.Start();
                    }
                    catch (Exception e)
                    {

                    }
                    System.GC.Collect();
                }
                listener.Stop();
            }
            catch (Exception e)
            {

            }
        }
        #endregion

        public class StateObject
        {
            public Socket workSocket = null;
            public const int BufferSize = 1024;
            public byte[] buffer = new byte[BufferSize];
            public StringBuilder sb = new StringBuilder();
        }

        public void StartListening()
        {
            try
            {
                if (cn == null)
                {
                    string ls_connectinfo;
                    OrgCode = "002";
                    if (getcnParms(OrgCode, out ls_connectinfo))
                    {
                        cn = new OracleConnection(ls_connectinfo);
                        dt = new DataTable();
                        dt = GetDataTable(@"select WebserviceIP,ServiceIP,Host,webservicehost,timers,flag from tb_wms_store where rownum = 1");
                        webserviceip = dt.Rows[0][0].ToString();
                        ipaddress = dt.Rows[0][1].ToString();
                        host = Convert.ToInt32(dt.Rows[0][2].ToString());
                        webservicehost = Convert.ToInt32(dt.Rows[0][3].ToString());
                        timers = Convert.ToInt32(dt.Rows[0][4].ToString());
                        ls_interfaceflag = dt.Rows[0][5].ToString();
                        timers = timers * 1000;
                    }
                }
                byte[] bytes = new Byte[1024];
                IPAddress ipAddress = IPAddress.Parse(ipaddress);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, host);
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (true)
                {
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                WriteLog("Server1", DateTime.Now, e.Message.ToString(), null, null);
            }
        }

        public void WriteLog(string ls_operuser, DateTime ls_opertime, string ls_text, string ls_readtext, string ls_sendtext)
        {
            string ls_returnmsg;
            if(ls_interfaceflag != "1")
            {
                return;
            }
            try
            {
                ls_returnmsg = SqlDataTableCommit(@"insert into Tb_Wms_InterfaceLog (LOGGUID,
                                           LOGID,
                                           LOGTEXT,
                                           OPERDATE,
                                           OPERUSER,
                                           REMARK,
                                           SENDTO,
                                           READTO)
                                           values
                                           (
                                           (select createguid from dual),
                                           (select nvl(max(logid),0) + 1 from Tb_Wms_InterfaceLog),
                                           '" + ls_text + @"',               
                                           sysdate,               
                                           '" + ls_operuser + @"',               
                                           null,               
                                           '" + ls_sendtext + @"',               
                                           '" + ls_readtext + @"'
                                           )");
            }
            catch (Exception e)
            {
                WriteLog("Server2", DateTime.Now, e.Message.ToString(), null, null);
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                allDone.Set();
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);
                StateObject state = new StateObject();
                state.workSocket = handler;
                //接口IP地址需要屏蔽
                bool exists = ((IList)sc).Contains(handler);
                if (exists)
                { }
                else
                {
                    co = new connectobject();
                    co.Scpro = handler;
                    co.Scstatus = "0";
                    _objectdo(co, "0", handler);
                    WriteLog("Server88", DateTime.Now, "**********|sc|**********add:" + ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), null);
                }
                if (state.workSocket.DontFragment == true)
                {
                    _setstatus(handler, "0");
                    WriteLog("Server3", DateTime.Now, "Geting for a connection1..." + ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), null);
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    _setstatus(handler, "0");
                    ls_curip = ((System.Net.IPEndPoint)handler.RemoteEndPoint).Address.ToString();
                    WriteLog("Server4", DateTime.Now, "Geting for a connection2..." + ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), null);
                    SendControl(ls_curip, handler);
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
            catch (Exception e)
            {
                WriteLog("Server5", DateTime.Now, e.Message.ToString(), null, null);
            }
        }

        #region 控制器类
        public class control
        {
            string controlip;//控制器IP
            public string Controlip
            {
                get { return controlip; }
                set { controlip = value; }
            }
            string status;//控制器状态1表示正在执行指令，WEBSERVICE在等待返回值状态
            public string Status
            {
                get { return status; }
                set { status = value; }
            }
            string webserviceip;//WEBSERVICE地址，记录地址是为后续服务器得到返回值后寻找通讯的WEBSERVICE
            public string Webserviceip
            {
                get { return webserviceip; }
                set { webserviceip = value; }
            }
        }
        #endregion

        #region 连接对象类
        public class connectobject
        {
            Socket scpro;//连接对象
            public Socket Scpro
            {
                get { return scpro; }
                set { scpro = value; }
            }
            string scstatus;//状态1表示已结束接收
            public string Scstatus
            {
                get { return scstatus; }
                set { scstatus = value; }
            }
        }
        #endregion

        public string ChangeCrc2(string ls_order)
        {
            string ls_neworder = "";
            if (ls_order.Length <= 6)
            {
                ls_neworder = ls_order;
                return ls_neworder;
            }
            ls_order = ls_order.Substring(4, ls_order.Length - 6);
            ls_order = ls_order.Replace("7D5E", "7E");
            ls_order = ls_order.Replace("7D5D", "7D");
            ls_neworder = "7EAA" + ls_order + "7E";
            return ls_neworder;
        }

        public void ReadCallback(IAsyncResult ar)
        {
            string ls_func;//功能号
            string ls_controlip;//控制器IP
            string ls_order = "";//指令
            Socket scnew = null;
            Socket scnew1 = null;
            String content = String.Empty;
            int bytesRead = 0;
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            #region 异常捕获--获取接收信息
            try
            {
                ls_curip = ((System.Net.IPEndPoint)handler.RemoteEndPoint).Address.ToString();
                bytesRead = handler.EndReceive(ar);
            }
            catch (Exception e)
            {
                if (handler != null)
                {
                    _objectdo(null, "1", handler);
                    WriteLog("server511", DateTime.Now, "**********|sc|**********remove:" + e.Message.ToString(), ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), null);
                    RollbackTrans();
                    return;
                }
                WriteLog("Server8", DateTime.Now, e.Message.ToString(), null, null);
                RollbackTrans();
                return;
            }
            #endregion
            if (bytesRead > 0)
            {
                _setstatus(handler, "1");
                #region 异常捕获--解析指令
                try
                {
                    if (((System.Net.IPEndPoint)handler.RemoteEndPoint).Address.ToString() != webserviceip)
                    {
                        state.sb.Append(ToHexString(state.buffer));//Label十六进制通讯指令
                    }
                    else
                    {
                        state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));//WEBSERVICE通讯指令
                    }
                    content = state.sb.ToString();
                    WriteLog("Server001", DateTime.Now, "得到指令：" + content, null, null);
                }
                catch (Exception e)
                {
                    if (handler != null)
                    {
                        _objectdo(null, "1", handler);
                        WriteLog("Server140", DateTime.Now, "**********|sc|**********remove:" + e.Message.ToString(), ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), null);
                        RollbackTrans();
                        return;
                    }
                    WriteLog("Server10", DateTime.Now, "解析指令失败：" + e.Message.ToString(), null, null);
                    RollbackTrans();
                    return;
                }
                #endregion
                #region 异常捕获--WEBSERVICE
                try
                {
                    #region 来自WEBSERVICE接口的信息
                    if (content.IndexOf("@@") > -1)//@@表示来自WEBSERVICE接口的信息
                    {
                        if (cn == null)
                        {
                            string ls_connectinfo;
                            OrgCode = "002";
                            if (getcnParms(OrgCode, out ls_connectinfo))
                            {
                                cn = new OracleConnection(ls_connectinfo);
                                dt = new DataTable();
                                dt = GetDataTable(@"select WebserviceIP,ServiceIP,Host,webservicehost,timers,flag from tb_wms_store where rownum = 1");
                                ipaddress = dt.Rows[0][1].ToString();
                                webserviceip = dt.Rows[0][0].ToString();
                                host = Convert.ToInt32(dt.Rows[0][2].ToString());
                                webservicehost = Convert.ToInt32(dt.Rows[0][3].ToString());
                                timers = Convert.ToInt32(dt.Rows[0][4].ToString());
                                ls_interfaceflag = dt.Rows[0][5].ToString();
                                timers = timers * 1000;
                            }
                        }
                        WriteLog("Server11", DateTime.Now, content, ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), null);
                        foreach (var item in lt)
                        {
                            if (((control)item).Status == "1" && ((control)item).Controlip == ((System.Net.IPEndPoint)handler.RemoteEndPoint).Address.ToString())
                            {
                                Send(handler, "该控制器正在执行任务，请稍后继续......");//下发信息到WEBSERVICE
                                RollbackTrans();
                                return;
                            }
                        }
                        ls_func = content.Substring(0, content.IndexOf("@@"));
                        #region 下发预处理命令3001
                        if (ls_func == "3001")
                        {
                            ls_controlip = content.Substring(6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((connectobject)item).Scpro.RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = ((connectobject)item).Scpro;
                                    break;
                                }
                            }
                            ls_order = "7EAA017D5E807E";
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString();
                            ct.Status = "1";
                            lt.Add(ct);
                            WriteLog("server402", DateTime.Now, "**********|lt|**********add:" + ct.Webserviceip, ct.Webserviceip, null);
                            //下发预处理命令
                        }
                        #endregion
                        #region 下发拣货命令3002
                        else if (ls_func == "3002")
                        {
                            ls_controlip = content.Substring(6, content.LastIndexOf("@@") - 6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((connectobject)item).Scpro.RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = ((connectobject)item).Scpro;
                                    break;
                                }
                            }
                            ls_order = content.Substring(content.LastIndexOf("@@") + 2);
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString();
                            ct.Status = "1";
                            lt.Add(ct);
                            WriteLog("server403", DateTime.Now, "**********|lt|**********add:" + ct.Webserviceip, ct.Webserviceip, null);
                            //下发拣货命令
                        }
                        #endregion
                        #region 下发盘点命令3003
                        else if (ls_func == "3003")
                        {
                            ls_controlip = content.Substring(6, content.LastIndexOf("@@") - 6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((connectobject)item).Scpro.RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = ((connectobject)item).Scpro;
                                    break;
                                }
                            }
                            ls_order = content.Substring(content.LastIndexOf("@@") + 2);
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString();
                            ct.Status = "1";
                            lt.Add(ct);
                            WriteLog("server404", DateTime.Now, "**********|lt|**********add:" + ct.Webserviceip, ct.Webserviceip, null);
                            //下发盘点命令
                        }
                        #endregion
                        #region 下发补货命令3004
                        else if (ls_func == "3004")
                        {
                            ls_controlip = content.Substring(6, content.LastIndexOf("@@") - 6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((connectobject)item).Scpro.RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = ((connectobject)item).Scpro;
                                    break;
                                }
                            }
                            ls_order = content.Substring(content.LastIndexOf("@@") + 2);
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString();
                            ct.Status = "1";
                            lt.Add(ct);
                            WriteLog("server405", DateTime.Now, "**********|lt|**********add:" + ct.Webserviceip, ct.Webserviceip, null);
                            //下发补货命令
                        }
                        #endregion
                        #region 下发完成下发命令3005
                        else if (ls_func == "3005")
                        {
                            ls_controlip = content.Substring(6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((connectobject)item).Scpro.RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = ((connectobject)item).Scpro;
                                    break;
                                }
                            }
                            ls_order = "7EAA057F437E";
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString();
                            ct.Status = "1";
                            lt.Add(ct);
                            WriteLog("server406", DateTime.Now, "**********|lt|**********add:" + ct.Webserviceip, ct.Webserviceip, null);
                            //下发完成下发命令
                        }
                        #endregion
                        #region 下发电子标签自检命令3006
                        else if (ls_func == "3006")
                        {
                            ls_controlip = content.Substring(6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((connectobject)item).Scpro.RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = ((connectobject)item).Scpro;
                                    break;
                                }
                            }
                            ls_order = "7EAA063F427E";
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString();
                            ct.Status = "1";
                            lt.Add(ct);
                            WriteLog("server407", DateTime.Now, "**********|lt|**********add:" + ct.Webserviceip, ct.Webserviceip, null);
                            //下发电子标签自检命令
                        }
                        #endregion
                        #region 下发电子标签地址显示命令3007
                        else if (ls_func == "3007")
                        {
                            ls_controlip = content.Substring(6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((connectobject)item).Scpro.RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = ((connectobject)item).Scpro;
                                    break;
                                }
                            }
                            ls_order = "7EAA07FE827E";
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString();
                            ct.Status = "1";
                            lt.Add(ct);
                            WriteLog("server408", DateTime.Now, "**********|lt|**********add:" + ct.Webserviceip, ct.Webserviceip, null);
                            //下发电子标签地址显示命令
                        }
                        #endregion
                        #region 下发关闭电子标签地址显示命令3008
                        else if (ls_func == "3008")
                        {
                            ls_controlip = content.Substring(6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((connectobject)item).Scpro.RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = ((connectobject)item).Scpro;
                                    break;
                                }
                            }
                            ls_order = "7EAA08BE867E";
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString();
                            ct.Status = "1";
                            lt.Add(ct);
                            WriteLog("server409", DateTime.Now, "**********|lt|**********add:" + ct.Webserviceip, ct.Webserviceip, null);
                            //下发关闭电子标签地址显示命令
                        }
                        #endregion
                        #region 下发复位命令3009
                        else if (ls_func == "3009")
                        {
                            ls_controlip = content.Substring(6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((connectobject)item).Scpro.RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = ((connectobject)item).Scpro;
                                    break;
                                }
                            }
                            ls_order = "7EAA097F467E";
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString();
                            ct.Status = "1";
                            lt.Add(ct);
                            WriteLog("server410", DateTime.Now, "**********|lt|**********add:" + ct.Webserviceip, ct.Webserviceip, null);
                            //下发复位命令
                        }
                        #endregion
                        StateObject statenew = new StateObject();
                        statenew.workSocket = scnew;
                        WriteLog("Server12", DateTime.Now, ls_order, null, ((System.Net.IPEndPoint)((Socket)scnew).RemoteEndPoint).ToString());
                        Send(scnew, ls_order);
                        CommitTrans();
                        if (_getstatus(scnew) == "1")
                        {
                            _setstatus(scnew, "0");
                            scnew.BeginReceive(statenew.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), statenew);
                        }
                        return;
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    if (handler != null)
                    {
                        _objectdo(null, "1", handler);
                        WriteLog("Server120", DateTime.Now, "**********|sc|**********Remove:" + e.Message.ToString(), ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), null);
                        RollbackTrans();
                        return;
                    }
                    WriteLog("Server14", DateTime.Now, e.Message.ToString(), null, null);
                    RollbackTrans();
                    return;
                }
                #endregion
                #region 异常捕获--控制器 来自控制器的信息
                try
                {
                    #region 来自控制器的信息
                    if (content.Substring(0, 4) == "7EAA")//表示来自控制器的信息
                    {
                        content = ChangeCrc2(content);
                        if (cn == null)
                        {
                            string ls_connectinfo;
                            OrgCode = "002";
                            if (getcnParms(OrgCode, out ls_connectinfo))
                            {
                                cn = new OracleConnection(ls_connectinfo);
                                dt = new DataTable();
                                dt = GetDataTable(@"select WebserviceIP,ServiceIP,Host,webservicehost,timers,flag from tb_wms_store where rownum = 1");
                                ipaddress = dt.Rows[0][1].ToString();
                                webserviceip = dt.Rows[0][0].ToString();
                                host = Convert.ToInt32(dt.Rows[0][2].ToString());
                                webservicehost = Convert.ToInt32(dt.Rows[0][3].ToString());
                                timers = Convert.ToInt32(dt.Rows[0][4].ToString());
                                ls_interfaceflag = dt.Rows[0][5].ToString();
                                timers = timers * 1000;
                            }
                        }
                        content = content.Substring(0, content.IndexOf("7E000000") + 2);
                        WriteLog("Server15", DateTime.Now, content, ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), null);
                        #region 电子标签主动上报
                        #region 拣货完成上行0A
                        if (content.Substring(4, 2) == "0A")//拣货完成上行
                        {
                            string ls_add;
                            string ls_state;
                            long ll_waresum;
                            long[] ll_location;
                            string ls_code;
                            int[] ll_warenum;
                            string ls_returnmsg = "";
                            string[] ls_inparam = null;
                            string[] ls_inparamtype = null;
                            string[] ls_inparamvalue = null;
                            string[] ls_outparam = null;
                            string[] ls_outparamtype = null;
                            string ls_labelguid = null;
                            string ls_pickguid = null;
                            string ls_xml = "";
                            ls_order = "7EAA0E3E847E";//主机应答
                            WriteLog("Server16", DateTime.Now, ls_order, null, ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString());
                            Send(handler, ls_order);
                            ls_add = Convert.ToString(Convert.ToInt32(content.Substring(14, 2), 16)).PadLeft(2, '0');//电子标签地址
                            ls_code = content.Substring(6, 8);//业务ID
                            ls_state = Convert.ToString(Convert.ToInt32(content.Substring(16, 2), 16)).PadLeft(2, '0');//处理状态
                            if (ls_state == "08" || ls_state == "09")
                            { }
                            else
                            {
                                if (ls_state == "01")
                                {
                                    WriteLog("Server", DateTime.Now, "执行成功", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "02")
                                {
                                    WriteLog("Server", DateTime.Now, "执行失败", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "03")
                                {
                                    WriteLog("Server", DateTime.Now, "任务已存在", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "04")
                                {
                                    WriteLog("Server", DateTime.Now, "任务不匹配", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "05")
                                {
                                    WriteLog("Server", DateTime.Now, "执行失败，控制正在执行其他命令", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "06")
                                {
                                    WriteLog("Server", DateTime.Now, "发送业务电子标签未响应  （电子标签故障 或者 线路故障）", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "07")
                                {
                                    WriteLog("Server", DateTime.Now, "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）", null, null);
                                    RollbackTrans();
                                    return;
                                }
                            }
                            ll_waresum = Convert.ToInt32(content.Substring(18, 2), 16);//货位总数
                            ll_location = new long[ll_waresum];
                            ll_warenum = new int[ll_waresum];

                            dt = new DataTable();
                            dt = GetDataTable(@"select labelguid from Tb_Wms_Label where labelcode = '" + ls_add + @"' and controlguid in(select controlguid from tb_wms_control where 
                                    ip = '" + ((System.Net.IPEndPoint)handler.RemoteEndPoint).Address.ToString() + "')");
                            ls_labelguid = dt.Rows[0][0].ToString();
                            dt = new DataTable();
                            dt = GetDataTable("select pickguid from Tb_Wms_LabelPick where LabelPickGUID in(select distinct LabelPickGUID from Tb_Wms_LabelPickDetail where BusinessID = '" + ls_code + "')");
                            ls_pickguid = dt.Rows[0][0].ToString();

                            ls_returnmsg = SqlDataTable("update Tb_Wms_PickDetail set ConfirmFlag = '2' where pickguid = '" + ls_pickguid + "' and LabelGUID = '" + ls_labelguid + "' and confirmflag = '0'");
                            //执行存储过程（判断任务是否完成，完成则批量处理）
                            ls_inparam = new string[2];
                            ls_inparam[0] = "as_pickguid";
                            ls_inparam[1] = "as_operuser";
                            ls_inparamtype = new string[2];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_inparamvalue = new string[2];
                            ls_inparamvalue[0] = ls_pickguid;
                            ls_inparamvalue[1] = "Label";

                            ls_outparam = new string[2];
                            ls_outparam[0] = "as_returncode";
                            ls_outparam[1] = "as_returnmsg";
                            ls_outparamtype = new string[2];
                            ls_outparamtype[0] = "varchar";
                            ls_outparamtype[1] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_completepick", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 0, true);
                            if (ls_returnmsg != "TRUE")
                            {
                                WriteLog("Server17", DateTime.Now, "Error:" + ls_returnmsg, null, null);
                                RollbackTrans();
                                return;
                            }
                            CommitTrans();
                            SendControl(ls_curip, handler);
                            return;
                        }
                        #endregion
                        #region 盘点完成上行0B
                        if (content.Substring(4, 2) == "0B")//盘点完成上行
                        {
                            string ls_add;
                            string ls_state;
                            long ll_waresum;
                            long[] ll_location;
                            string ls_code;
                            int[] ll_warenum;
                            string ls_returnmsg = "";
                            string[] ls_inparam = null;
                            string[] ls_inparamtype = null;
                            string[] ls_inparamvalue = null;
                            string[] ls_outparam = null;
                            string[] ls_outparamtype = null;
                            string ls_labelguid = null;
                            string ls_pickguid = null;
                            string ls_xml = "";
                            ls_order = "7EAA0E3E847E";//主机应答
                            WriteLog("Server170", DateTime.Now, ls_order, null, ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString());
                            Send(handler, ls_order);
                            ls_add = Convert.ToString(Convert.ToInt32(content.Substring(14, 2), 16)).PadLeft(2, '0');//电子标签地址
                            ls_code = content.Substring(6, 8);//业务ID
                            ls_state = Convert.ToString(Convert.ToInt32(content.Substring(16, 2), 16)).PadLeft(2, '0');//处理状态
                            if (ls_state == "08" || ls_state == "09")
                            { }
                            else
                            {
                                if (ls_state == "01")
                                {
                                    WriteLog("Server", DateTime.Now, "执行成功", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "02")
                                {
                                    WriteLog("Server", DateTime.Now, "执行失败", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "03")
                                {
                                    WriteLog("Server", DateTime.Now, "任务已存在", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "04")
                                {
                                    WriteLog("Server", DateTime.Now, "任务不匹配", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "05")
                                {
                                    WriteLog("Server", DateTime.Now, "执行失败，控制正在执行其他命令", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "06")
                                {
                                    WriteLog("Server", DateTime.Now, "发送业务电子标签未响应  （电子标签故障 或者 线路故障）", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "07")
                                {
                                    WriteLog("Server", DateTime.Now, "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）", null, null);
                                    RollbackTrans();
                                    return;
                                }
                            }
                            ll_waresum = Convert.ToInt32(content.Substring(18, 2), 16);//货位总数
                            ll_location = new long[ll_waresum];
                            ll_warenum = new int[ll_waresum];

                            //更新电子标签拣货单明细的确认数量、确认人、确认时间、确认状态等
                            dt = new DataTable();
                            dt = GetDataTable(@"select labelguid from Tb_Wms_Label where labelcode = '" + ls_add + @"' and controlguid in(select controlguid from tb_wms_control where 
                                    ip = '" + ((System.Net.IPEndPoint)handler.RemoteEndPoint).Address.ToString() + "')");
                            ls_labelguid = dt.Rows[0][0].ToString();
                            dt = new DataTable();
                            dt = GetDataTable("select pickguid from Tb_Wms_LabelPick where LabelPickGUID in(select distinct LabelPickGUID from Tb_Wms_LabelPickDetail where BusinessID = '" + ls_code + "')");
                            ls_pickguid = dt.Rows[0][0].ToString();

                            ls_returnmsg = SqlDataTable("update Tb_Wms_InventDetail set status = '2' where InventGUID = '" + ls_pickguid + "' and LabelGUID = '" + ls_labelguid + "' and status = '0'");
                            //执行存储过程（判断任务是否完成，完成则批量处理）
                            ls_inparam = new string[2];
                            ls_inparam[0] = "as_pickguid";
                            ls_inparam[1] = "as_operuser";
                            ls_inparamtype = new string[2];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamtype[1] = "varchar";
                            ls_inparamvalue = new string[2];
                            ls_inparamvalue[0] = ls_pickguid;
                            ls_inparamvalue[1] = "Label";

                            ls_outparam = new string[2];
                            ls_outparam[0] = "as_returncode";
                            ls_outparam[1] = "as_returnmsg";
                            ls_outparamtype = new string[2];
                            ls_outparamtype[0] = "varchar";
                            ls_outparamtype[1] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_completeinvent", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 0, true);
                            if (ls_returnmsg != "TRUE")
                            {
                                WriteLog("Server18", DateTime.Now, "Error:" + ls_returnmsg, null, null);
                                RollbackTrans();
                                return;
                            }
                            CommitTrans();
                            SendControl(ls_curip, handler);
                            return;
                        }
                        #endregion
                        #region 补货完成上行0C
                        if (content.Substring(4, 2) == "0C")//补货完成上行
                        {
                            string ls_add;
                            string ls_state;
                            string ls_code;
                            string ls_returnmsg = "";
                            string[] ls_inparam = null;
                            string[] ls_inparamtype = null;
                            string[] ls_inparamvalue = null;
                            string[] ls_outparam = null;
                            string[] ls_outparamtype = null;
                            string ls_groundguid = null;
                            string ls_xml = "";
                            ls_order = "7EAA0E3E847E";//主机应答
                            WriteLog("Server770", DateTime.Now, ls_order, null, ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString());
                            Send(handler, ls_order);
                            ls_add = Convert.ToString(Convert.ToInt32(content.Substring(14, 2), 16)).PadLeft(2, '0');//电子标签地址
                            ls_code = content.Substring(6, 8);//业务ID
                            ls_state = Convert.ToString(Convert.ToInt32(content.Substring(16, 2), 16)).PadLeft(2, '0');//处理状态
                            if (ls_state == "08" || ls_state == "09")
                            { }
                            else
                            {
                                if (ls_state == "01")
                                {
                                    WriteLog("Server", DateTime.Now, "执行成功", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "02")
                                {
                                    WriteLog("Server", DateTime.Now, "执行失败", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "03")
                                {
                                    WriteLog("Server", DateTime.Now, "任务已存在", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "04")
                                {
                                    WriteLog("Server", DateTime.Now, "任务不匹配", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "05")
                                {
                                    WriteLog("Server", DateTime.Now, "执行失败，控制正在执行其他命令", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "06")
                                {
                                    WriteLog("Server", DateTime.Now, "发送业务电子标签未响应  （电子标签故障 或者 线路故障）", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                if (ls_state == "07")
                                {
                                    WriteLog("Server", DateTime.Now, "轮询业务电子标签未响应  （电子标签故障 或者 线路故障）", null, null);
                                    RollbackTrans();
                                    return;
                                }
                                WriteLog("Server", DateTime.Now, "无法识别的响应代码", null, null);
                                return;
                            }

                            //更新标志
                            dt = new DataTable();
                            dt = GetDataTable("select groundguid from Tb_Wms_LabelGround where LabelGroundGUID = (select LabelGroundGUID from tb_wms_labelgrounddetail where BusinessID2 = '" + ls_code + "')");
                            ls_groundguid = dt.Rows[0][0].ToString();

                            ls_returnmsg = SqlDataTable("update Tb_Wms_BoxGroundDetail set ConfirmFlag = '2' where GroundGUID = '" + ls_groundguid + "' and ConfirmFlag = '0' and transportboxguid =(select transportboxguid from Tb_Wms_LabelGroundDetail where businessid2 = '" + ls_code + "')");
                            if (ls_returnmsg != "TRUE")
                            {
                                WriteLog("Server20", DateTime.Now, "Error:" + ls_returnmsg, null, null);
                                RollbackTrans();
                                return;
                            }
                            ls_returnmsg = SqlDataTable("update tb_wms_label set status = '0' where LabelCode ='" + ls_add + "'");
                            if (ls_returnmsg != "TRUE")
                            {
                                WriteLog("Server21", DateTime.Now, "Error:" + ls_returnmsg, null, null);
                                RollbackTrans();
                                return;
                            }

                            //执行存储过程（增加库存、设置状态等）
                            ls_inparam = new string[1];
                            ls_inparam[0] = "as_groundguid";
                            ls_inparamtype = new string[1];
                            ls_inparamtype[0] = "varchar";
                            ls_inparamvalue = new string[1];
                            ls_inparamvalue[0] = ls_groundguid;

                            ls_outparam = new string[2];
                            ls_outparam[0] = "as_returncode";
                            ls_outparam[1] = "as_returnmsg";
                            ls_outparamtype = new string[2];
                            ls_outparamtype[0] = "varchar";
                            ls_outparamtype[1] = "varchar";
                            ls_returnmsg = Doprocedure("miawms.sp_completeground ", ls_inparam, ls_inparamvalue, ls_inparamtype, ls_outparam, ls_outparamtype, out ls_xml, 1016, true);

                            if (ls_returnmsg != "TRUE")
                            {
                                WriteLog("Server22", DateTime.Now, "Error:" + ls_returnmsg, null, null);
                                RollbackTrans();
                                return;
                            }
                            CommitTrans();
                            SendControl(ls_curip, handler);
                            return;
                        }
                        #endregion
                        #region 灯塔通信故障上行0D
                        if (content.Substring(4, 2) == "0D")//灯塔通信故障上行
                        {
                            ls_order = "7EAA0E3E847E";//主机应答
                            Send(handler, ls_order);
                            WriteLog("Server", DateTime.Now, "灯塔通信故障", null, null);
                            RollbackTrans();
                            return;
                        }
                        #endregion
                        #region 上行服务器应答0E
                        if (content.Substring(4, 2) == "0E")//上行服务器应答
                        {
                            ls_order = "7EAA0E3E847E";//主机应答
                            Send(handler, ls_order);
                            WriteLog("Server", DateTime.Now, "上行服务器应答", null, null);
                            RollbackTrans();
                            return;
                        }
                        #endregion
                        #region 状态显示标签故障0F
                        if (content.Substring(4, 2) == "0F")//状态显示标签故障
                        {
                            ls_order = "7EAA0E3E847E";//主机应答
                            Send(handler, ls_order);
                            WriteLog("Server", DateTime.Now, "状态显示标签故障", null, null);
                            RollbackTrans();
                            return;
                        }
                        #endregion
                        #endregion
                        #region 电子标签响应
                        if (lt.Count == 0)
                        {
                            WriteLog("Server9999", DateTime.Now, "电子标签响应回复对象消失" + content, null, null);
                        }
                        else
                        {
                            //解析消息内容
                            foreach (var item in lt)
                            {
                                WriteLog("Server9999", DateTime.Now, "监控一" + ((control)item).Status + "||" + ((control)item).Controlip + "||" + ((System.Net.IPEndPoint)handler.RemoteEndPoint).Address.ToString(), null, null);
                                if (((control)item).Status == "1" && ((control)item).Controlip == ((System.Net.IPEndPoint)handler.RemoteEndPoint).Address.ToString())
                                {
                                    scnew1 = null;
                                    string ls_webserviceip;
                                    ls_webserviceip = ((control)item).Webserviceip;
                                    foreach (var item1 in sc)
                                    {
                                        WriteLog("Server9999", DateTime.Now, "监控二" + ((System.Net.IPEndPoint)((connectobject)item1).Scpro.RemoteEndPoint).ToString() + "||" + ls_webserviceip, null, null);
                                        if (((System.Net.IPEndPoint)((connectobject)item1).Scpro.RemoteEndPoint).ToString() == ls_webserviceip && ((System.Net.IPEndPoint)((connectobject)item1).Scpro.RemoteEndPoint).Address.ToString() == webserviceip)
                                        {
                                            scnew1 = ((connectobject)item1).Scpro;
                                            sc.Remove(item1);//webservice--服务器--控制器--服务器--webservice流程完成后，清除连接对象资源
                                            WriteLog("server519", DateTime.Now, "**********|sc|**********remove:" + ((System.Net.IPEndPoint)scnew1.RemoteEndPoint).ToString(), ((System.Net.IPEndPoint)scnew1.RemoteEndPoint).ToString(), null);
                                            Send(scnew1, content);//下发控制器答复信息到WEBSERVICE
                                            WriteLog("Server24", DateTime.Now, content, null, ((System.Net.IPEndPoint)scnew1.RemoteEndPoint).ToString());
                                            lt.Remove(item);
                                            WriteLog("server501", DateTime.Now, "**********|lt|**********remove:" + ((System.Net.IPEndPoint)scnew1.RemoteEndPoint).ToString(), ((System.Net.IPEndPoint)scnew1.RemoteEndPoint).ToString(), null);
                                            //System.Threading.Thread.Sleep(2000);
                                            //scnew1.Shutdown(SocketShutdown.Both);
                                            //scnew1.Close();
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        CommitTrans();
                        if (content == "7EAA0501C3207E")//完成下发应答后，结束连接，电子标签自动产生新的上报连接
                        {
                            _objectdo(null, "1", handler);
                            WriteLog("Server777", DateTime.Now, "Ending..." + ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), null);
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                        }
                        return;
                        #endregion
                    }
                    #endregion
                    else
                    {
                        WriteLog("Server25", DateTime.Now, "不能识别的命令来源" + content, ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), null);
                        RollbackTrans();
                        return;
                    }
                }
                catch (Exception e)
                {
                    if (handler != null)
                    {
                        _objectdo(null, "1", handler);
                        WriteLog("Server101", DateTime.Now, "**********|sc|**********remove:" + e.Message.ToString(), ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), null);
                        RollbackTrans();
                        return;
                    }
                    WriteLog("Server27", DateTime.Now, e.Message.ToString(), null, null);
                    RollbackTrans();
                    return;
                }
                #endregion
            }
        }

        public void Send(Socket handler, String data)
        {
            try
            {
                byte[] byteData = strToHexByte(data);
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
            }
            catch (Exception e)
            {
                _objectdo(null, "1", handler);
                WriteLog("Server150", DateTime.Now, "**********|sc|**********remove:" + e.Message.ToString(), ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString(), null);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                if (handler != null && handler.Connected)
                {
                    int bytesSent = handler.EndSend(ar);
                }
            }
            catch (Exception e)
            {
                WriteLog("Server30", DateTime.Now, e.Message.ToString(), null, null);
            }
        }

        public string SendControl(string as_controlip, Socket handler)
        {
            try
            {
                string ls_controlguid = "";
                string ls_order;
                string ls_return = "FALSE";
                dt = new DataTable();
                dt = GetDataTable(@"select controlguid from tb_wms_control where ip = '" + as_controlip + "'");
                if (dt.Rows.Count > 0)
                {
                    ls_controlguid = dt.Rows[0][0].ToString();
                    dt = new DataTable();
                    dt = GetDataTable(@"select count(1) from tb_wms_control where controlguid = '" + ls_controlguid + "' and status = '0'");
                    if (Convert.ToInt32(dt.Rows[0][0].ToString()) > 0)
                    {
                        dt = new DataTable();
                        dt = GetDataTable(@"select controlguid
                          from Tb_Wms_Pick t
                         where (Status = getpickstatus('已通知拣货') or
                               status = getpickstatus('已通知拣货【PDA作业】')
                                or Status = getpickstatus('已领取拣货'))
                           and controlguid = '" + ls_controlguid + @"'
                           and NatureGUID in(select NatureGUID from Tb_Wms_StoreAreaNature where naturecode = '002')
                        union all
                        select controlguid
                          from Tb_Wms_Invent t
                         where (status = getinventstatus('已通知盘点') or status = getinventstatus('已领取盘点')) 
                           and controlguid  = '" + ls_controlguid + @"'
                           and StoreAreaGUID in(select storeareaguid from tb_wms_storearea where storeareaguid = t.storeareaguid and natureguid in(select NatureGUID from Tb_Wms_StoreAreaNature where naturecode = '002'))");
                        if (dt.Rows.Count > 0)
                        {
                            #region 下发预处理命令
                            ls_order = "7EAA017D5E807E";
                            SqlDataTableCommit(@"UPDATE Tb_Wms_Control
                        				        SET status = '4'
                        				        Where controlguid = '" + ls_controlguid + "'");
                            StateObject statenew = new StateObject();
                            WriteLog("Server31", DateTime.Now, ls_order, null, ((System.Net.IPEndPoint)((Socket)handler).RemoteEndPoint).ToString());
                            Send(handler, ls_order);
                            CommitTrans();
                            #endregion 下发预处理命令
                        }
                    }
                }
                return ls_return;
            }
            catch (Exception e)
            {
                WriteLog("Server32", DateTime.Now, e.Message.ToString(), null, null);
                return e.Message.ToString();
            }
        }

        #region 数据操作相关
        public string Doprocedure(string proname, string[] inparam, string[] inparamvalue, string[] inparamtype, string[] outparam, string[] outparamtype, out string ls_returnxml, int func, bool ib_commit)
        {
            int returncode = -1;
            string returnmsg = "";
            if (cn == null)
            {
                string ls_connectinfo;
                OrgCode = "002";
                if (getcnParms(OrgCode, out ls_connectinfo))
                {
                    cn = new OracleConnection(ls_connectinfo);
                    dt = new DataTable();
                    dt = GetDataTable(@"select WebserviceIP,ServiceIP,Host,webservicehost,timers,flag from tb_wms_store where rownum = 1");
                    ipaddress = dt.Rows[0][1].ToString();
                    webserviceip = dt.Rows[0][0].ToString();
                    host = Convert.ToInt32(dt.Rows[0][2].ToString());
                    webservicehost = Convert.ToInt32(dt.Rows[0][3].ToString());
                    timers = Convert.ToInt32(dt.Rows[0][4].ToString());
                    ls_interfaceflag = dt.Rows[0][5].ToString();
                    timers = timers * 1000;
                }
            }
            cmd = cn.CreateCommand();
            OracleParameter param = null;
            ls_returnxml = "";
            DataSet ds = new DataSet();
            dt = new DataTable();
            OracleDataAdapter da = null;
            string ls_cursorname = "";
            string ls_text = "";
            ls_returnxml = "";
            try
            {
                Open();
                BeginTrans();
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                cmd.CommandText = proname;
                cmd.CommandType = CommandType.StoredProcedure;
                for (int i = 0; i < inparam.Length; i++)
                {
                    if (inparamtype[i] == "int")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(inparam[i], OracleDbType.Int32, 8));
                    }
                    else if (inparamtype[i] == "varchar")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(inparam[i], OracleDbType.Varchar2, 400));
                    }
                    param.Direction = ParameterDirection.Input;
                    param.Value = inparamvalue[i];
                }

                for (int i = 0; i < outparam.Length; i++)
                {
                    if (outparamtype[i] == "int")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(outparam[i], OracleDbType.Int32, 4));
                    }
                    else if (outparamtype[i] == "varchar")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(outparam[i], OracleDbType.Varchar2, 400));
                    }
                    else if (outparamtype[i] == "cursor")
                    {
                        param = cmd.Parameters.Add(new OracleParameter(outparam[i], OracleDbType.RefCursor, 400));
                        ls_cursorname = outparam[i];
                    }
                    param.Direction = ParameterDirection.Output;
                    param.Value = ls_text.PadRight(400, ' ');
                }
                cmd.ExecuteNonQuery();
                returncode = Convert.ToInt32(cmd.Parameters["as_returncode"].Value.ToString());
                returnmsg = Convert.ToString(cmd.Parameters["as_returnmsg"].Value.ToString());
                if (returncode != 0)
                {
                    RollbackTrans();
                    return (returnmsg);
                }

                if (ib_commit)
                {
                    //CommitTrans();
                }
                return ("TRUE");
            }
            catch (Exception ex)
            {
                RollbackTrans();
                returnmsg = ex.Message.ToString();
                return (returnmsg);
            }
            finally
            {
                cmd = null;
                if (da != null)
                {
                    da.Dispose();
                }
                if (ds != null)
                {
                    ds.Dispose();
                }
                if (dt != null)
                {
                    dt.Dispose();
                }
            }
        }

        public string SqlDataTable(string strSql)
        {
            string ls_return = "";
            cmd = null;
            try
            {
                Open();
                BeginTrans();
                cmd = new OracleCommand();
                cmd.Connection = this.cn;
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                cmd.CommandText = strSql;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ls_return = ex.Message.ToString();
                RollbackTrans();
                cmd = null;
                return (ls_return);
            }
            finally
            {
                cmd = null;
            }
            return ("TRUE");
        }

        public string SqlDataTableCommit(string strSql)
        {
            string ls_return = "";
            OracleCommand cmdd = null;
            try
            {
                Open();
                BeginTrans();
                cmdd = new OracleCommand();
                cmdd.Connection = this.cn;

                if (inTransaction)
                {
                    cmdd.Transaction = trans;
                }

                cmdd.CommandText = strSql;
                cmdd.ExecuteNonQuery();

                if (!inTransaction)
                {
                    //CommitTrans();
                }
            }
            catch (Exception ex)
            {
                ls_return = ex.Message.ToString();
                if (!inTransaction && cn.State.ToString().ToUpper() == "OPEN")
                {
                    RollbackTrans();
                }
                cmdd = null;
                return (ls_return);
            }
            finally
            {
                cmdd = null;
            }
            return ("TRUE");
        }

        public DataSet GetDataSet(string QueryString)
        {
            DataSet ds = null;
            cmd = null;
            OracleDataAdapter ad = null;
            try
            {
                Open();
                BeginTrans();
                cmd = new OracleCommand();
                cmd.Connection = this.cn;
                if (inTransaction)
                {
                    cmd.Transaction = trans;
                }
                ds = new DataSet();
                ad = new OracleDataAdapter();
                cmd.CommandText = QueryString;
                ad.SelectCommand = cmd;
                ad.Fill(ds);
                /* CommitTrans(); */
            }
            catch (Exception ex)
            {
                RollbackTrans();
                throw ex;
            }
            finally
            {
                cmd = null;
                ad = null;
            }
            return (ds);
        }

        public DataTable GetDataTable(string QueryString)
        {
            DataSet ds = GetDataSet(QueryString);
            if (ds != null)
            {
                if (ds.Tables.Count > 0)
                {
                    return (ds.Tables[0]);
                }
                else
                {
                    return (new DataTable());
                }
            }
            else
            {
                return (new DataTable());
            }
        }

        public void Open()
        {
            if (cn.State.ToString().ToUpper() != "OPEN")
                this.cn.Open();
        }

        public void Close()
        {
            if (cn.State.ToString().ToUpper() == "OPEN")
            {
                this.cn.Close();
            }
        }

        public void DisPose()
        {
            if (cn.State.ToString().ToUpper() == "OPEN")
            {
                this.cn.Close();
            }

            this.cn.Dispose();
            this.cn = null;
        }

        protected void BeginTrans()
        {
            if (trans == null)
            {
                trans = null;
                trans = cn.BeginTransaction();
                inTransaction = true;
            }
        }

        protected void CommitTrans()
        {
            if (trans != null)
            {
                try
                {
                    trans.Commit();
                    inTransaction = false;
                    Close();
                }
                catch { }
            }
            else
            {
                if (cn != null)
                {
                    Close();
                }
            }
        }

        protected void RollbackTrans()
        {
            if (trans != null)
            {
                try
                {
                    trans.Rollback();
                    Close();
                }
                catch { }
            }
            else
            {
                if (cn != null)
                {
                    Close();
                }
            }

        }

        /*
 * / <summary>
 * / 获取数据库联接参数串
 * / </summary>
 * / <param name="OrgCode"></param>
 * / <returns></returns>
 */
        public bool getcnParms(string orgcode, out string mess)
        {
            XmlTextReader txtReader = new XmlTextReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OrgCode + ".xml"));
            try
            {
                /* 找到符合的节点获取需要的属性值 */
                while (txtReader.Read())
                {
                    txtReader.MoveToElement();
                    if (txtReader.Name == "org")
                    {
                        if (txtReader.GetAttribute("code") == OrgCode)
                        {
                            DBServer = txtReader.GetAttribute("DBServer");
                            Port = txtReader.GetAttribute("PORT");
                            Host = txtReader.GetAttribute("HOST");
                            Server_Name = txtReader.GetAttribute("SERVICE_NAME");
                            UserID = txtReader.GetAttribute("UserID");
                            PassWord = txtReader.GetAttribute("PassWord");
                            break;
                        }
                        if (txtReader.NodeType.ToString() == "EndElement")
                        {
                            break;
                        }
                    }
                }
                if (DBServer == "")
                {
                    mess = "获取机构" + OrgCode + "的数据库联接参数错误，请检查配置文件！";
                    return (false);
                }
                else
                {
                    mess = "Data Source=(DESCRIPTION =    (ADDRESS_LIST =      (ADDRESS = (PROTOCOL = TCP)(HOST = " + Host + ")(PORT = " + Port + "))    )    (CONNECT_DATA =      (SERVER = DEDICATED)      (SERVICE_NAME = " + Server_Name + ")    )  );Persist Security Info=True;User ID=" + UserID + ";Password=" + PassWord + ";";
                    return (true);
                }
            }
            catch (Exception e)
            {
                if (e.Message.ToString().Contains("未能找到文件"))
                {
                    mess = "服务器没有找到机构" + OrgCode + "的数据库联接配置参数！";
                }
                else
                {
                    mess = "获取机构" + OrgCode + "的数据库联接参数错误：" + e.Message.ToString();
                }
                return (false);
            }
            finally
            {
                txtReader.Close();
            }
        }

        /// <summary>
        /// 连接对象处理
        /// </summary>
        /// <param name="cnobj">控制器对象</param>
        /// <param name="coobj">连接对象</param>
        /// <param name="flag">1表示remove</param>
        /// <param name="scobj">检索出对象</param>
        private void _objectdo(connectobject coobj, string flag, Socket scobj)
        {
            if (flag == "0")
            {
                if (coobj != null)
                {
                    sc.Add(coobj);
                }
            }
            else if (flag == "1")
            {
                foreach (var item in sc)
                {
                    if (((connectobject)item).Scpro == scobj)
                    {
                        sc.Remove(item);
                        return;
                    }
                }
            }
        }

        private void _setstatus(Socket scobj, string flag)
        {
            foreach (var item in sc)
            {
                if (((connectobject)item).Scpro == scobj)
                {
                    ((connectobject)item).Scstatus = flag;
                    if (flag == "0")
                    {
                        WriteLog("server", DateTime.Now, "receive:" + ((System.Net.IPEndPoint)scobj.RemoteEndPoint).ToString(), ((System.Net.IPEndPoint)scobj.RemoteEndPoint).ToString(), null);
                    }
                    else if (flag == "1")
                    {
                        WriteLog("server", DateTime.Now, "noreceive:" + ((System.Net.IPEndPoint)scobj.RemoteEndPoint).ToString(), ((System.Net.IPEndPoint)scobj.RemoteEndPoint).ToString(), null);
                    }
                    return;
                }
            }
        }

        private string _getstatus(Socket scobj)
        {
            foreach (var item in sc)
            {
                if (((System.Net.IPEndPoint)((connectobject)item).Scpro.RemoteEndPoint).Address.ToString() == ((System.Net.IPEndPoint)scobj.RemoteEndPoint).
Address.ToString())
                {
                    return ((connectobject)item).Scstatus;
                }
            }
            return "1";
        }

        /*
         * / <summary>
         * / 服务器名
         * / </summary>
         */
        private string DBServer = "";

        /*
         * / <summary>
         * / 服务器名
         * / </summary>
         */
        private string Port = "";

        /*
         * / <summary>
         * / 服务器名
         * / </summary>
         */
        private string Host = "";

        /*
         * / <summary>
         * / 服务器名
         * / </summary>
         */
        private string Server_Name = "";

        /*
         * / <summary>
         * / 数据库名
         * / </summary>
         */
        private string DBName = "";

        /*
         * / <summary>
         * / 数据库连接用户ID
         * / </summary>
         */
        private string UserID = "";

        /*
         * / <summary>
         * / 数据库连接用户密码
         * / </summary>
         */
        private string PassWord = "";

        /*
         * / <summary>
         * / 当前用户所在的机构代码
         * / </summary>
         */
        private string OrgCode = "";
        #endregion
    }

}
