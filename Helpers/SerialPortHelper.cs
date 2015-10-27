using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Runtime.Remoting.Messaging;

namespace Helpers
{
    public class SerialPortHelper
    {
        SerialPort sp;
        private delegate void DelSend(string msg);
        
        public SerialPortHelper(string portName, int baudRate)
        {
            sp = new SerialPort(portName);
            sp.BaudRate = baudRate;
        }

        public void BeginSend0x(string message)
        {
            DelSend delSend = new DelSend(Send0x);
            delSend.BeginInvoke(message, new AsyncCallback(Send0xComplete), null);
        }

        public void Send0x(string message)
        {
            //处理数字转换
            string[] strArray = Messageto0x(message);
            Send0x(strArray);
            LogHelper.GetInstance().ShowMsg("send to com port: " + message);
        }

        private void Send0xComplete(IAsyncResult result)
        {
            AsyncResult _result = (AsyncResult)result;
            DelSend delSend = (DelSend)_result.AsyncDelegate;
            delSend.EndInvoke(_result);
        }

        public void SendString(string message)
        {
            //处理数字转换
            if (!sp.IsOpen)
            {
                sp.Open();
            }
            sp.Write(message);
        }

        private void Send0x(string[] strArray)
        {
            int byteBufferLength = strArray.Length;
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i] == "")
                {
                    byteBufferLength--;
                }
            }
            // int temp = 0;
            byte[] byteBuffer = new byte[byteBufferLength];
            int ii = 0;
            for (int i = 0; i < strArray.Length; i++)        //对获取的字符做相加运算
            {

                Byte[] bytesOfStr = Encoding.Default.GetBytes(strArray[i]);

                int decNum = 0;
                if (strArray[i] == "")
                {
                    //ii--;     //加上此句是错误的，下面的continue以延缓了一个ii，不与i同步
                    continue;
                }
                else
                {
                    decNum = Convert.ToInt32(strArray[i], 16); //atrArray[i] == 12时，temp == 18 
                }

                try    //防止输错，使其只能输入一个字节的字符
                {
                    byteBuffer[ii] = Convert.ToByte(decNum);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                ii++;
            }
            if (!sp.IsOpen)
            {
                sp.Open();
            }
            sp.Write(byteBuffer, 0, byteBuffer.Length);
        }

        private string[] Messageto0x(string message)
        {
            string sendBuf = message;
            string sendnoNull = sendBuf.Trim();
            string sendNOComma = sendnoNull.Replace(',', ' ');    //去掉英文逗号
            string sendNOComma1 = sendNOComma.Replace('，', ' '); //去掉中文逗号
            string strSendNoComma2 = sendNOComma1.Replace("0x", "");   //去掉0x
            strSendNoComma2.Replace("0X", "");   //去掉0X
            return strSendNoComma2.Split(' ');
        }
    }
}
