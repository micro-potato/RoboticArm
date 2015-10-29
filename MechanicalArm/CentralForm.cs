using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ArmController;
using Helpers;

namespace MechanicalArm
{
    public partial class CentralForm : Form, ILog
    {
        private RobotHandler _robotHandler;
        private ArmHandler _armHandler;
        private delegate void deleString(string arg);
        private System.Timers.Timer _reachObjectTimer;
        private int _reachTimerTicked = 0;
        private int _pressState = 0;

        //moving hanoi
        int _reachedTime = 3;
        int _powerSettedTime = 6;
        int _carryFinishedTime = 8;
        public CentralForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LogHelper.GetInstance().RegLog(this);
            InitConfig();
            InitReachTimer();
            InitArmHandler();
            InitRobotController();
            InitHanoiTime();
        }

        private void InitHanoiTime()
        {
            _reachedTime = ConfigHelper.GetInstance().ReachedTime;
            _powerSettedTime = ConfigHelper.GetInstance().PowerSettedTime;
            _carryFinishedTime = ConfigHelper.GetInstance().CarryFinishedTime;
        }

        private void InitReachTimer()
        {
            _reachObjectTimer = new System.Timers.Timer(1000);
            _reachObjectTimer.Elapsed += new System.Timers.ElapsedEventHandler(ReachTimerTicked);
        }

        /// <summary>
        /// 抓取过程计时
        /// </summary>
        void ReachTimerTicked(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_reachTimerTicked < _reachedTime)//wait for move
            {
                _reachTimerTicked++;
            }
            else if (_reachTimerTicked == _reachedTime+1)//notify power to Robot
            {
                _robotHandler.NotifyPower(_pressState);
                _reachTimerTicked++;
                LogHelper.GetInstance().ShowMsg("通知机械臂下移，等待下移完成。。。");
            }
            else if (_reachTimerTicked < _powerSettedTime)//wait for set power
            {
                _reachTimerTicked++;
            }
            else if (_reachTimerTicked == _powerSettedTime)//set power
            {
                _robotHandler.SetPower(_pressState);
                _reachTimerTicked++;
                LogHelper.GetInstance().ShowMsg("变更电磁铁状态，等待抓取/放置完成。。。");
            }
            else if (_reachTimerTicked < _carryFinishedTime)//wait for hold or realese finish
            {
                _reachTimerTicked++;
            }
            else if (_reachTimerTicked == _carryFinishedTime)//finish,give back control right to glove
            {
                _reachObjectTimer.Stop();
                _robotHandler.ReachtoObject(0);
                SetReflecttoArmMove(true);
                _reachTimerTicked = 0;
                LogHelper.GetInstance().ShowMsg("抓取/放置完成，将机械臂移动控制权交还手套。。。");
            }
        }

        private void InitConfig()
        {
            ConfigHelper.GetInstance().ResolveConfig(System.Windows.Forms.Application.StartupPath + @"\config.xml");
        }

        private void InitRobotController()
        {
            _robotHandler=new RobotHandler("172.31.1.147",30002);
            //_armHandler = new ArmHandler("127.0.0.1", 6065);
        }

        private void InitArmHandler()
        {
            _armHandler = new ArmHandler(12345, Helpers.ConfigHelper.GetInstance().JointTimer);
            _armHandler.OffsetUpdated += new ArmHandler.DeleOffsetUpdated(OnOffsetUpdated);
            _armHandler.ButtonStateUpdated += new ArmHandler.DeleButtonStareUpdated(OnButtonStateUpdated);
            //_jointHandler.DataIn += new JointHandler.GetData(Joint_DataIn);
        }

        void OnButtonStateUpdated(int currentState)
        {
            currentState = (currentState - 1) * -1;
            _pressState = currentState;
            if (_pressState == 0)
                LogHelper.GetInstance().ShowMsg("Button Released");
            else if(_pressState==1)
                LogHelper.GetInstance().ShowMsg("Button Pressed");
            BeginReachtoObject();
        }

        void OnOffsetUpdated(double[] offsetData)
        {
            //LogHelper.GetInstance().ShowMsg("Move offset-----------------------------------------" + string.Join("|",offsetData));
            _robotHandler.MoveArm(offsetData);
        }

        #region raw data from glove
        void Joint_DataIn(string data)
        {
            this.Invoke(new deleString(DealData),data);
            LogHelper.GetInstance().ShowMsg("Joint data in----------" + data);
        }

        void DealData(string data)
        {
            //_armHandler.Move(data);
        }
        #endregion

        private void BeginReachtoObject()
        {
            SetReflecttoArmMove(false);
            _robotHandler.ReachtoObject(1);//begin move
            _reachObjectTimer.Start();
            LogHelper.GetInstance().ShowMsg("通知机械臂移动到最近点上方。。。");
        }

        private void SetReflecttoArmMove(bool isReflecttoArmMove)
        {
            if (!isReflecttoArmMove)
            {
                try
                {
                    _armHandler.OffsetUpdated -= new ArmHandler.DeleOffsetUpdated(OnOffsetUpdated);
                }
                catch(Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            else
            {
                try
                {
                    _armHandler.OffsetUpdated += new ArmHandler.DeleOffsetUpdated(OnOffsetUpdated);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        /// <summary>
        /// 开始获取手套数据，操作机械臂
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartAdjust_Click(object sender, EventArgs e)
        {
            _armHandler.StartAdjust();
        }

        private void EndAdjust_Click(object sender, EventArgs e)
        {
            _armHandler.EndAdjust();
            string toSend = "<A1>0</A1><A2>0</A2><A3>0</A3><A4>0</A4><A5>0</A5><A6>0</A6><A7>0</A7>";
            _robotHandler.MoveArm(toSend);
        }

        public void ShowLog(string msg)
        {
            this.Invoke(new deleString(SetText), msg);
        }

        private void SetText(string text)
        {
            this.InfoText.AppendText(text + "\n");
            if (InfoText.Lines.Length > 5000)
            {
                InfoText.Clear();
            }
            this.InfoText.ScrollToCaret();
        }

        #region Test
        private void Move_Click(object sender, EventArgs e)
        {
            string toSend = string.Format("<A1>{0}</A1><A2>{1}</A2><A3>{2}</A3><A4>{3}</A4><A5>{4}</A5><A6>{5}</A6><A7>{6}</A7>", textBox1.Text.Trim(), textBox2.Text.Trim(), textBox3.Text.Trim(), textBox4.Text.Trim(), textBox5.Text.Trim(), textBox6.Text.Trim(), textBox7.Text.Trim());
            _robotHandler.MoveArm(toSend);
        }

        private void PressDown_Click(object sender, EventArgs e)
        {
            _pressState = 1;
            BeginReachtoObject();
        }

        private void PressUp_Click(object sender, EventArgs e)
        {
            _pressState = 0;
            BeginReachtoObject();
        }

        private void Reach_Click(object sender, EventArgs e)
        {
            try
            {
                _robotHandler.ReachtoObject(1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ClearLog_Click(object sender, EventArgs e)
        {
            this.InfoText.Clear();
        }
        #endregion
    }
}
