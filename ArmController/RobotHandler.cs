using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDLL;
using System.Threading;
using Helpers;

namespace ArmController
{
    public class RobotHandler
    {
        private AsyncClient asyncClient;
        private System.Timers.Timer asyncTimer = new System.Timers.Timer();
        private char[] m_endChar = new char[] { '\n'};
        string _ip;
        int _port;
        double _A1k, _A2k, _A3k, _A4k, _A2DownMax, _Y2excludeY1k, _P2excludeP1k;
        private double[] _prevOffset = new double[6];

        //Power
        SerialPortHelper _serialPortHelper;
        public RobotHandler(string ip, int port)
        {
            _ip = ip;
            _port = port;
            InitAngleKs();
            InitSocket(ip,port);
            try
            {
                _serialPortHelper = new SerialPortHelper(ConfigHelper.GetInstance().ComPort, 9600);
            }
            catch (Exception e)
            {
                LogHelper.GetInstance().ShowMsg("Init SerialPort fail:" + e.Message);
            }
        }

        private void InitAngleKs()
        {
            var config = ConfigHelper.GetInstance();
            _A1k = config.A1k;
            _A2k = config.A2k;
            _A3k = config.A3k;
            _A4k = config.A4k;
            _A2DownMax = config.A2DownMax;
            _Y2excludeY1k = config.Y2excludeY1k;
            _P2excludeP1k = config.P2excludeP1k;
        }

        #region Socket
        private void InitSocket(string ip,int port)
        {
            try
            {
                InitAsyncTimer();
                if (this.asyncClient != null)
                {
                    this.asyncClient.Dispose();
                    this.asyncClient.onConnected -= new AsyncClient.Connected(client_onConnected);
                    this.asyncClient.onDisConnect -= new AsyncClient.DisConnect(client_onDisConnect);
                    this.asyncClient.onDataByteIn -= new AsyncClient.DataByteIn(client_onDataByteIn);
                }
                asyncClient = new AsyncClient();
                asyncClient.onConnected += new AsyncClient.Connected(client_onConnected);
                asyncClient.Connect(ip, port);
                asyncClient.onDataByteIn += new AsyncClient.DataByteIn(client_onDataByteIn);
                asyncClient.onDisConnect += new AsyncClient.DisConnect(client_onDisConnect);
            }
            catch (Exception ex)
            {
                
            }
        }

        void client_onDataByteIn(byte[] SocketData)
        {
            string cmd = System.Text.Encoding.UTF8.GetString(SocketData);
            //this.Invoke(new DelSetText(SetText), new object[] { cmd });
            string[] dataList = cmd.Split(m_endChar, StringSplitOptions.RemoveEmptyEntries);
            foreach (string data in dataList)
            {
                //this.Invoke(new DelDataDeal(DataDeal), new object[] { data });
            }
        }

        void client_onConnected()
        {
            try
            {
                Thread.Sleep(100);
                asyncTimer.Stop();
                //string message = string.Format("{0}连线!", ip);
                //this.Invoke(new DelSetText(SetText), new object[] { message });
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        void client_onDisConnect()
        {
            string message = string.Format("{0}断线!",_ip);
            //this.Invoke(new DelSetText(SetText), new object[] { message });
            asyncTimer.Start();
        }

        private void asyncTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ReConnect();
        }

        private void ReConnect()
        {
            asyncClient.Dispose();
            asyncClient.Connect(_ip, _port);
        }

        private void InitAsyncTimer()
        {
            asyncTimer = new System.Timers.Timer();
            asyncTimer.Interval = 1500;
            asyncTimer.Elapsed += new System.Timers.ElapsedEventHandler(asyncTimer_Elapsed);
        }
        #endregion       

        #region Move
        /// <summary>
        /// 根据偏移移动机械臂
        /// </summary>
        /// <param name="offsetData">距离基准的偏移</param>
        public void MoveArm(double[] offsetData)
        {
            if (!ArmMoved(offsetData))
                return;

            var correctedOffsetY1 = CalcYaw(offsetData[2], 1);//avoid offset larger than 180
            double offsetA1 = correctedOffsetY1* _A1k;//big arm H value

            var offsetP1 = offsetData[1];
            double offsetA2 = CalcPitch(offsetP1, _A2k);//big arm V value

            /*小臂移动，包括A3和A4的转动角度*/
            var correctedOffsetY2 = CalcYaw(offsetData[5], 1);//avoid offset larger than 180
            var offsetP2 = offsetData[4];
            var correctedOffsetY2excludeY1 = CalcYaw((correctedOffsetY2 + correctedOffsetY1) * _Y2excludeY1k, 1);//y2独立于y1的偏移量
            var correctedOffsetP2excludP1 = (offsetP2 + offsetP1)*_P2excludeP1k;//p2独立于p1的偏移量

            //消除当p2移动时y2不必要的移动
            correctedOffsetY2excludeY1 += correctedOffsetP2excludP1;

            //消除胳膊肘朝外拐的姿势
            if (correctedOffsetY2excludeY1 > 0)
            {
                correctedOffsetY2excludeY1 = 0;
            }

            LogHelper.GetInstance().ShowMsg("Y2独立偏移量：=============" + correctedOffsetY2excludeY1.ToString());
            LogHelper.GetInstance().ShowMsg("P2独立偏移量：=============" + correctedOffsetP2excludP1.ToString());

            double offsetA4 = CalcA4(correctedOffsetP2excludP1, correctedOffsetY2excludeY1, _A4k);
            LogHelper.GetInstance().ShowMsg("A4移动：=============" + offsetA4);
            double offsetA3 = 0;

            if (Math.Abs(offsetA4) < 20)//小臂移动过小，忽略
            {
                offsetA3 = 0;
                offsetA4 = 0;
            }
            else
            {
                offsetA3  = CalcA3(correctedOffsetY2excludeY1, correctedOffsetP2excludP1, _A3k);
                LogHelper.GetInstance().ShowMsg("小臂夹角：=============" + offsetA3);
            }

            //处理越界,机械臂最大移动范围
            offsetA1 = SetBoundary(offsetA1, 170);
            offsetA2 = SetBoundary(offsetA2, 120);
            offsetA3 = SetBoundary(offsetA3, 90);
            offsetA4 = SetBoundary(offsetA4, 120);
            if (offsetA2 > _A2DownMax)//A2下移
            {
                offsetA2 = _A2DownMax;
            }

            //位置基本水平时，保持A7方向竖直
            if (Math.Abs(offsetA3) < 15)
            {
                offsetA3 = 0;
                LogHelper.GetInstance().ShowMsg(string.Format("A3 from {0} to 0", offsetA3)); ;
            }

            string datatoSend = string.Format("<A1>{0}</A1><A2>{1}</A2><A3>{2}</A3><A4>{3}</A4><A5>{4}</A5><A6>{5}</A6><A7>{6}</A7>|", offsetA1.ToString(), offsetA2.ToString(), "0", offsetA4, "0", "0", "0");
            asyncClient.Send(datatoSend);
            offsetData.CopyTo(_prevOffset,0);
            LogHelper.GetInstance().ShowMsg("send to IIWA:=============" + datatoSend + "\n");
        }

        private double SetBoundary(double currentValue, int maxValue)
        {
            if (Math.Abs(currentValue) > maxValue)
            {
                currentValue = maxValue * (currentValue / Math.Abs(currentValue));
            }
            return currentValue;
        }

        private bool ArmMoved(double[] offsetData)
        {
            double movedValue=0;
            movedValue = Math.Abs(offsetData[2] - _prevOffset[2]) + Math.Abs(offsetData[1] - _prevOffset[1]) + Math.Abs(offsetData[5] - _prevOffset[5]) + Math.Abs(offsetData[4] - _prevOffset[4]);
            LogHelper.GetInstance().ShowMsg("距上次移动了："+movedValue.ToString());
            if (movedValue >= 1) return true;
            else return false;
        }

        /// <summary>
        /// 计算发送给机械臂的pitch角度
        /// </summary>
        /// <param name="pitch">来自手套的picth</param>
        /// <param name="k">修正系数</param>
        /// <returns></returns>
        private double CalcPitch(double pitch, double k)
        {
            return pitch * k;
        }

        /// <summary>
        /// 计算发送给机械臂的yaw角度
        /// </summary>
        /// <param name="yaw">来自手套的yaw角度</param>
        /// <param name="k">修正系数</param>
        /// <returns></returns>
        private double CalcYaw(double yaw,double k)
        {
            var offsetY = yaw;
            if (Math.Abs(offsetY) >= 180)
            {
                if (offsetY > 0)
                {
                    offsetY = offsetY - 360 + 1;
                }
                else
                {
                    offsetY = 360 + offsetY - 1;
                }
            }
            return offsetY*k;
        }

        /// <summary>
        /// A3轴旋转度数
        /// </summary>
        /// <param name="offsetY2excludeY1">小臂水平偏移</param>
        /// <param name="offsetP2excludP1">小臂竖直偏移</param>
        /// <returns></returns>
        private double CalcA3(double offsetY2excludeY1, double offsetP2excludP1,double A3k)
        {
            int dir = offsetP2excludP1 * offsetY2excludeY1 >= 0 ? 1 : -1;//A3轴偏转方向，+-对称
            double s = Math.Atan(offsetP2excludP1 / offsetY2excludeY1);
            double offsetA3 = s * 180 / Math.PI;
            return offsetA3 * dir * A3k;
        }

        /// <summary>
        /// A4轴旋转度数
        /// </summary>
        /// <param name="offsetP2excludP1">小臂水平偏移</param>
        /// <param name="offsetY2excludeY1">小臂竖直偏移</param>
        /// <param name="_A4k"></param>
        /// <returns></returns>
        private double CalcA4(double offsetP2excludP1, double offsetY2excludeY1, double _A4k)
        {
            double offsetLength = Math.Sqrt((Math.Pow(offsetY2excludeY1, 2) + Math.Pow(offsetP2excludP1, 2)));//移动距离
            //double offsetLength = Math.Abs(offsetY2excludeY1);//移动距离,只考虑小臂水平
            //int dir = offsetY2excludeY1 >= 0 ? 1 : -1;
            int dir = offsetP2excludP1 >= 0 ? -1 : 1;
            return offsetLength * dir * _A4k;
        }

        public void MoveArm(string data)
        {
            try
            {
                asyncClient.Send(data + "|");
                LogHelper.GetInstance().ShowMsg("send to IIWA:=============" + data + "\n");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion
        public void NotifyPower(int powerType)
        {
            asyncClient.Send(string.Format("<Power>{0}</Power>|", powerType.ToString()));
        }

        public void SetPower(int powerType)//0 off,1 on
        {
            try
            {
                if (powerType == 1)//power on
                    _serialPortHelper.Send0x("55 01 12 00 00 00 01 69");
                else
                    _serialPortHelper.Send0x("55 01 11 00 00 00 01 68");
                LogHelper.GetInstance().ShowMsg("Set power:" + powerType);
            }
            catch (Exception e)
            {
                LogHelper.GetInstance().ShowMsg("Set Power fail:" + e.Message);
            }
        }

        public void ReachtoObject(int reachType)//0 cancel,1 reach
        {
            string msg = string.Format("<Reach>{0}</Reach>|", reachType);
            asyncClient.Send(msg);
            LogHelper.GetInstance().ShowMsg("send to IIWA:" + msg);
        }
    }
}
