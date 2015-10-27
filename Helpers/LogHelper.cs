using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers
{
    public interface ILog
    {
        void ShowLog(string msg);
    }
    public class LogHelper
    {
        private static LogHelper logHelper;
        private ILog ilog;
        private LogHelper()
        {

        }
        public static LogHelper GetInstance()
        {
            if(logHelper==null)
            {
                logHelper=new LogHelper();
            }
            return logHelper;
        }

        public void RegLog(ILog log)
        {
            ilog = log;
        }

        public void ShowMsg(string msg)
        {
            if (ilog != null)
            {
                ilog.ShowLog(msg);
            }
        }
    }
}
