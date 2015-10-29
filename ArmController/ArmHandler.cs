using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using UDPDLL;
using Helpers;


namespace ArmController
{
    public class ArmHandler
    {
        private AsyncUDP asyncUDP = new AsyncUDP();

        public delegate void GetData(string data);
        public event GetData DataIn;

        private int _port;

        private string _latestDataString;
        private System.Timers.Timer _notifyTimer;//对手套数据的响应时间

        //Adjust
        private double[] _baseData;//校验后的基准数据
        private double[] _offsetData;//与基准数据的偏移量
        private double[] _latestData;
        private System.Timers.Timer _adjustTimer = new Timer(3000);
        private bool _isAdjusted = false;
        public delegate void DeleOffsetUpdated(double[] offsetData);
        public event DeleOffsetUpdated OffsetUpdated;
        public delegate void DeleButtonStareUpdated(int currentState);
        public event DeleButtonStareUpdated ButtonStateUpdated;
        int _currentButtonState = 1;

        private int _timerInterval = 0;
        
        public ArmHandler(int port,int timer=0)
        {
            _port = port;
            _baseData = new double[6];
            _latestData = new double[6];
            _offsetData = new double[6];

            _timerInterval = timer;

            InitUDP();
            InitNotifyTimer();
            _adjustTimer.Elapsed += new ElapsedEventHandler(AjustedFinish);
        }

        public void StartAdjust()
        {
            _isAdjusted = false;
            _adjustTimer.Stop();
            _adjustTimer.Start();
        }

        void AjustedFinish(object sender, ElapsedEventArgs e)
        {
            try
            {
                _adjustTimer.Stop();
                LogHelper.GetInstance().ShowMsg("Base Coordinates string==========" + _latestDataString);
                _latestData = ResolveCoordinates(_latestDataString);
                _latestData.CopyTo(_baseData, 0);
                _isAdjusted = true;
                _notifyTimer.Start();
            }
            catch(Exception ex)
            {
                LogHelper.GetInstance().ShowMsg("校验失败：" + ex.Message);
                EndAdjust();
            }
        }

        public void EndAdjust()
        {
            _isAdjusted = false;
            _notifyTimer.Stop();
        }

        public void UpdateOffset()
        {
            try
            {
                
            }
            catch(Exception e)
            {

            }
        }

        private void InitNotifyTimer()
        {
            if (_timerInterval == 0)//fire datain instantly
            {
                return;
            }
            else
            {
                _notifyTimer = new Timer(_timerInterval);
                _notifyTimer.Elapsed += new ElapsedEventHandler(DataNotifyTimer_Elapsed);
                _notifyTimer.Start();
            }
        }

        void DataNotifyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_latestDataString))
                _latestData = ResolveCoordinates(_latestDataString);
            if (_isAdjusted)
            {
                UpdateButton();
                CalcOffset();
                NotifyOffset();
            }
            else
                UpdateBaseCoodinate();
        }

        private void UpdateButton()
        {
            var stateGotton = GetButtonState(_latestDataString);
            if (stateGotton != _currentButtonState)
            {
                if (ButtonStateUpdated != null)
                {
                    ButtonStateUpdated(stateGotton);
                }
                _currentButtonState = stateGotton;
            }
        }

        private void UpdateBaseCoodinate()
        {
            if(!_latestData.IsValidJoint())
                return;
            if (_baseData.IsValidJoint())
            {
                double[] averData = _baseData.AverJointValue(_latestData);
                averData.CopyTo(_baseData, 0);
            }
            else
                _latestData.CopyTo(_baseData,0);
        }

        private void CalcOffset()
        {
            double[] offsetDatas = new double[6];
            try
            {
                for (int i = 0; i < 6; i++)
                {
                    offsetDatas[i] = Math.Round((_latestData[i] - _baseData[i]),1);
                }
                offsetDatas.CopyTo(_offsetData, 0);
            }
            catch(Exception e)
            {
                LogHelper.GetInstance().ShowMsg("Calc offset error:" + e.Message);
            }
        }

        private void NotifyOffset()
        {
            if (OffsetUpdated != null)
            {
                OffsetUpdated(_offsetData);
            }
        }

        private string AllAngleMsg(double[] angleData)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < angleData.Length; i++)
            {
                string angleValue=angleData[i].ToString();
                if (!string.IsNullOrEmpty(angleValue))
                {
                    sb.Append(string.Format("<A{0}>{1}</A{0}>", (i + 1).ToString(), angleValue));
                }
                else//no data in
                {
                    return string.Empty;
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// Resolve data from glove
        /// </summary>
        /// <param name="coorString">glove data string</param>
        /// <returns></returns>
        public double[] ResolveCoordinates(string coorString)
        {
            try
            {
                if (string.IsNullOrEmpty(coorString))
                {
                    return null;
                }
                double[] datas = new double[6];
                string[] coords = coorString.Split('|');
                for(int i=0;i<6;i++)
                {
                    datas[i] = double.Parse(coords[i].Split(':')[1]);
                }
                return datas;
            }
            catch (Exception e)
            {
                LogHelper.GetInstance().ShowMsg("Can't resolve data from glove:" + e.Message);
                throw new Exception(e.Message);
            }
        }

        public int GetButtonState(string coorString)
        {
            string stateString = coorString.Substring(coorString.LastIndexOf("|"));
            var state = stateString.Split(':')[1];
            if (string.IsNullOrEmpty(state))//error:no state in
                return _currentButtonState;
            return int.Parse(state);
        }

        #region 网络通讯(UDP)
        private void InitUDP()
        {
            if (asyncUDP.InitUDPServer(12345))
            {
                asyncUDP.OnDataIn += new AsyncUDP.DelDataIn(asyncUDP_OnDataIn);
                //LogHelper.GetInstance().ShowMsg("UDP Init");
            }
            else
            {
                LogHelper.GetInstance().ShowMsg("UDP端口被占用，请重新配置!");
            }
        }

        private void asyncUDP_OnDataIn(string ip, string data)
        {
            //LogHelper.GetInstance().ShowMsg(data);
            if (!string.IsNullOrEmpty(data))
            {
                _latestDataString = data;
                if (_timerInterval == 0)
                {
                    if (this.DataIn != null)
                    {
                        DataIn(_latestDataString);
                    }
                    if (_isAdjusted)
                    {
                        CalcOffset();
                        NotifyOffset();
                    }
                }
            }
        }  
        #endregion
    }

    static class DoubleArrayExtendMethods
    {
        public static bool IsValidJoint(this double[] joint)
        {
            for (int i = 0; i < joint.Length; i++)
            {
                if (joint[i] != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static double[] AverJointValue(this double[] joint,double[] targetArray)
        {
            double[] newArray = new double[joint.Length];
            for (int i = 0; i < joint.Length; i++)
            {
                var aver = (joint[i] + targetArray[i]) / 2;
                newArray[i] = aver;
            }
            return newArray;
        }
    }
}
