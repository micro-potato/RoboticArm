using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArmController
{
    public class HanoiPlayer
    {
        public enum MoveType { Hold, Relese };
        private int _x1Count, _x2Count, _x3Count;
        public delegate void deleLocate(int x1Count, int x2Count, int x3Count);
        public event deleLocate HanoiLocateUpdated;
        private RobotHandler _robotHandler;
        private System.Timers.Timer _moveArmTimer;
        private int timerTicked = 0;
        private MoveType _currentMoveType;

        public HanoiPlayer(RobotHandler roboteHandler)
        {
            _robotHandler = roboteHandler;
            _x1Count = 3;
            _x2Count = 0;
            _x3Count = 0;
            InitMoveTimer();
        }

        private void InitMoveTimer()
        {
            _moveArmTimer = new System.Timers.Timer(1000);
            _moveArmTimer.Elapsed += new System.Timers.ElapsedEventHandler(MoveArmTimer_Elapsed);
        }

        void MoveArmTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (timerTicked == 0)//start move
            {
                string coordinates = GetCoordinates();
                _robotHandler.MoveArm(coordinates);
                timerTicked++;
            }
            else if (timerTicked == 4)//set power
            {
                int powerState = 0;
                if (_currentMoveType == MoveType.Hold)
                    powerState = 1;
                else
                    powerState = 0;
                _robotHandler.SetPower(powerState);
                timerTicked++;
            }
            else if (timerTicked == 5)//finish,give back control right to glove
            {
                _moveArmTimer.Stop();
                timerTicked = 0;
                _robotHandler.SetSendState(true);
                EndHanoiDeal();
            }
        }

        private void EndHanoiDeal()//set count
        {
            throw new NotImplementedException();
        }

        private string GetCoordinates()
        {
            throw new NotImplementedException();
        }

        public void DealHanoi(MoveType moveType)
        {
            _currentMoveType = moveType;
            SetLocateInfo(_robotHandler.LatestOffet);
            _robotHandler.SetSendState(false);
            StartMoveArm();
        }

        private void StartMoveArm()
        {
            throw new NotImplementedException();
        }

        private void SetLocateInfo(double[] offsettoBase)
        {
            throw new NotImplementedException();
        }
    }
}
