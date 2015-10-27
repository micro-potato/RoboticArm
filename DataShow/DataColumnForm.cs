using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ArmController;
using System.Xml;
using System.IO;

namespace DataShow
{
    public partial class DataColumnForm : Form
    {
        private ArmHandler _jointHandler;
        public DataColumnForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.axShockwaveFlash1.Movie = Application.StartupPath + "\\main.swf";
            _jointHandler = new ArmHandler(12345);
            _jointHandler.DataIn += new ArmHandler.GetData(JointHandler_DataIn);
            //_jointHandler.OffsetUpdated += new JointHandler.DeleOffsetUpdated(OnOffsetUpdated);
        }

        void OnOffsetUpdated(double[] offsetData)
        {
            
        }

        void JointHandler_DataIn(string data)
        {
            var Datas = _jointHandler.ResolveCoordinates(data);
            if (Datas == null || Datas.Length == 0)//no data in
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            foreach (var valueName in Datas)
            {
                try
                {
                    var value = valueName;
                    sb.Append(value.ToString() + ",");
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
            var dataValue=sb.ToString().TrimEnd(',');
            CalFlash(dataValue);
        }

        private void CalFlash(string cmd)
        {
            this.axShockwaveFlash1.CallFunction(EncodeInvoke("datain", cmd));
        }

        private string EncodeInvoke(string Functionname, string arg)
        {
            StringBuilder sb = new StringBuilder();
            XmlTextWriter xw = new XmlTextWriter(new StringWriter(sb));

            xw.WriteStartElement("invoke");
            xw.WriteAttributeString("name", Functionname);
            xw.WriteAttributeString("returntype", "xml");

            xw.WriteStartElement("arguments");
            xw.WriteStartElement("string");   //此处直接创建string类型，没做别的类型判断
            xw.WriteString(arg);
            xw.WriteEndElement();
            xw.WriteEndElement();

            xw.WriteEndElement();

            xw.Flush();
            xw.Close();
            return sb.ToString();
        }
    }
}
