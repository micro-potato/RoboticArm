using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Helpers;
using ArmController;

namespace TestProject1
{
    /// <summary>
    /// UnitTest1 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        //
        // 编写测试时，可以使用以下附加特性:
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestCoord()
        {
            ArmHandler jh = new ArmHandler(12345);
            string testString = "roll:-74.60|pitch:8.41|yaw:-169|roll2:-33.45|pitch2:36.33|yaw2:104";
            double[] datas = jh.ResolveCoordinates(testString);
            Assert.AreEqual(-74.60, datas[0]);
            Assert.AreEqual(8.41, datas[1]);
            Assert.AreEqual(-11, datas[2]);
            Assert.AreEqual(-33.45, datas[3]);
            Assert.AreEqual(36.33, datas[4]);
            Assert.AreEqual(76, datas[5]);
        }

        [TestMethod]
        public void TestCalcOffset()
        {
            ArmHandler_Accessor target = new ArmHandler_Accessor(12345); // TODO: 初始化为适当的值
            ArmHandler jh = new ArmHandler(12345);
            string baseString = "roll:-74.60|pitch:8.41|yaw:-169.16|roll2:33.45|pitch2:36.33|yaw2:-104.82";
            string testString = "roll:-75.60|pitch:6.41|yaw:169.16|roll2:-33.45|pitch2:37.33|yaw2:-102.82";
            target._baseData = target.ResolveCoordinates(baseString);
            target._latestData = target.ResolveCoordinates(testString);
            target.CalcOffset();
            Assert.AreEqual( -1,target._offsetData[0]);
            Assert.AreEqual(-2, target._offsetData[1]);
            Assert.AreEqual(338.32, target._offsetData[2]);
            Assert.AreEqual(-66.9, target._offsetData[3]);
            Assert.AreEqual(1, target._offsetData[4]);
            Assert.AreEqual(2, target._offsetData[5]);
        }

        [TestMethod]
        public void TestConfig()
        {
            ConfigHelper.GetInstance().ResolveConfig(@"E:\code\code2015\KukaArm\MechanicalArm\MechanicalArm\bin\Debug\config.xml");
            //Assert.AreEqual(30, ConfigHelper.GetInstance().JointTimer);
            //Assert.AreEqual(0.75, ConfigHelper.GetInstance().A1k);
            //Assert.AreEqual(1.125, ConfigHelper.GetInstance().A2k);
            //Assert.AreEqual(1, ConfigHelper.GetInstance().A3k);
            //Assert.AreEqual(-1, ConfigHelper.GetInstance().A4k);
            //Assert.AreEqual(10, ConfigHelper.GetInstance().A2DownMax);
            //Assert.AreEqual(1, ConfigHelper.GetInstance().Y2excludeY1k);
            //Assert.AreEqual(1.125, ConfigHelper.GetInstance().P2excludeP1k);
            //Assert.AreEqual("COM4", ConfigHelper.GetInstance().ComPort);
            Assert.AreEqual(3, ConfigHelper.GetInstance().ReachedTime);
            Assert.AreEqual(6, ConfigHelper.GetInstance().PowerSettedTime);
            Assert.AreEqual(8, ConfigHelper.GetInstance().CarryFinishedTime);
        }

        [TestMethod]
        public void TestArcTan()
        {
            double x = 0;
            double y = 0;
            double s = Math.Atan(y / x);
            s = s * 180 / Math.PI;
            Assert.AreEqual(double.NaN, s);
        }

        [TestMethod]
        public void IsValidJoint()
        {
            double[] joint = new double[] {0,0,0,0,0,0 };
            Assert.AreEqual(false, joint.IsValidJoint());
        }
        [TestMethod]
        public void AverJointValue()
        {
            double[] joint = new double[] {1,1,1,1,1,1 };
            double[] newjoint = new double[] { 1, 3, 5, 7, 9, 11 };
            double[] averjoint = joint.AverJointValue(newjoint);
            Assert.AreEqual(1, averjoint[0]);
            Assert.AreEqual(2, averjoint[1]);
            Assert.AreEqual(3, averjoint[2]);
            Assert.AreEqual(4, averjoint[3]);
            Assert.AreEqual(5, averjoint[4]);
            Assert.AreEqual(6, averjoint[5]);
        }

        [TestMethod]
        public void TestGetButtonState()
        {
            string baseString = "roll:-74.60|pitch:8.41|yaw:-169.16|roll2:33.45|pitch2:36.33|yaw2:-104.82|button:1";
            ArmHandler a = new ArmHandler(12345);
            var state = a.GetButtonState(baseString);
            Assert.AreEqual(1, state);
        }
    }
}
