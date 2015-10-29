using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Helpers
{
    public class ConfigHelper
    {
        public string ComPort { get; set; }
        public int JointTimer { get; set; }
        public double A1k { get; set; }
        public double A2k { get; set; }
        public double A3k { get; set; }
        public double A4k { get; set; }
        public double A2DownMax { get; set; }
        public double Y2excludeY1k { get; set; }
        public double P2excludeP1k { get; set; }
        public int ReachedTime { get; set; }
        public int PowerSettedTime { get; set; }
        public int CarryFinishedTime { get; set; }
        

        private static ConfigHelper _configHelper;
        private ConfigHelper()
        {

        }

        public static ConfigHelper GetInstance()
        {
            if (_configHelper == null)
            {
                _configHelper = new ConfigHelper();
            }
            return _configHelper;
        }

        public void ResolveConfig(string configPath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(configPath);
            this.JointTimer = Convert.ToInt32(xmlDocument.SelectSingleNode("Data/JointTimer").InnerText);
            this.A1k = Convert.ToDouble(xmlDocument.SelectSingleNode("Data/A1k").InnerText);
            this.A2k = Convert.ToDouble(xmlDocument.SelectSingleNode("Data/A2k").InnerText);
            this.A3k = Convert.ToDouble(xmlDocument.SelectSingleNode("Data/A3k").InnerText);
            this.A4k = Convert.ToDouble(xmlDocument.SelectSingleNode("Data/A4k").InnerText);
            this.A2DownMax = Convert.ToDouble(xmlDocument.SelectSingleNode("Data/A2DownMax").InnerText);
            this.Y2excludeY1k = Convert.ToDouble(xmlDocument.SelectSingleNode("Data/Y2excludeY1k").InnerText);
            this.P2excludeP1k = Convert.ToDouble(xmlDocument.SelectSingleNode("Data/P2excludeP1k").InnerText);
            this.ComPort = xmlDocument.SelectSingleNode("Data/ComPort").InnerText;
            this.ReachedTime = Convert.ToInt32(xmlDocument.SelectSingleNode("Data/ReachedTime").InnerText);
            this.PowerSettedTime = Convert.ToInt32(xmlDocument.SelectSingleNode("Data/PowerSettedTime").InnerText);
            this.CarryFinishedTime = Convert.ToInt32(xmlDocument.SelectSingleNode("Data/CarryFinishedTime").InnerText);
        }
    }
}
