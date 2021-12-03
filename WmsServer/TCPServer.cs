using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Xml;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Net;
using System.Threading;


namespace WmsServer
{
    public class TCPServer
    {
        public int timers;
        public TcpListener listener = null;
        #region 实例注销
        private TcpClient _client = null;
           /// <summary>
        /// TcpClient对象
        /// </summary>
        public TcpClient client
        {
            set { this._client = value; }
        }

        #endregion

        public static byte[] StringToByte(string InString)
        {
            string[] ByteStrings;
            ByteStrings = InString.Split(" ".ToCharArray());
            byte[] ByteOut;
            ByteOut = new byte[ByteStrings.Length - 1];
            for (int i = 0; i == ByteStrings.Length - 1; i++)
            {
                ByteOut[i] = Convert.ToByte(("0x" + ByteStrings[i]));
            }
            return ByteOut;
        }

        public byte[] strToHexByte(string hexString)
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


        private void SendInfo(string str)
        {    
            NetworkStream tcpStream = _client.GetStream();
            byte[] messages = strToHexByte(str);
            tcpStream.WriteTimeout = timers;//发送时间
            tcpStream.Write(messages, 0, messages.Length); 
        }

        #region 主程序，供线程调用
        public void HandleConnection()
        {
            NetworkStream stream = _client.GetStream();
            string data = string.Empty;
            byte[] bytes = new byte[1024];
            stream.ReadTimeout = timers;//读取时间
            int length = stream.Read(bytes, 0, bytes.Length);
            if (length > 0)
            {
                data = ToHexString(bytes);
                //if (data.Substring(0, 4) == "3001")
                //{
                //    SendInfo(data.Substring(4, 14));
                //}
                ////Console.WriteLine("Control:" + data);
            }
            stream.Close();
            _client.Close();        
        }
        #endregion     
    }
}
