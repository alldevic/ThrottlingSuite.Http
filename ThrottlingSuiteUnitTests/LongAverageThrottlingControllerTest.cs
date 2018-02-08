#region "Copyright (C) Lenny Granovsky. 2012-2014"
//
//                Copyright (C) Lenny Granovsky. 2012-2014. 
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//    This program comes with ABSOLUTELY NO WARRANTY.
//    This is free software, and you are welcome to redistribute it
//    under certain conditions;
#endregion

using ThrottlingSuite.Modules;
using ThrottlingSuite.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace ThrottlingSuiteUnitTests
{
    /// <summary>
    ///This is a test class for AverageThrottlingControllerTest and is intended
    ///to contain all AverageThrottlingControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LongAverageThrottlingControllerTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for IsCallAllowed
        ///</summary>
        [TestMethod()]
        public void IsCallAllowedTest()
        {
            int timeIntervalMsec = 1000;
            int maxThreshold = 5;
            LongAverageThrottlingController controller = new LongAverageThrottlingController(timeIntervalMsec, maxThreshold, 300000);
            string requestSignature = Guid.NewGuid().ToString();
            List<DateTime> timestamps = new List<DateTime>(10);

            for (int ii = 0; ii < 10; ii++)
                timestamps.Add(DateTime.Now);

            bool expected = true;
            bool actual = true;
            List<TestItem> flow = GenerateTestFlow(10, 1000);
            DateTime requestTimestamp = DateTime.Now;
            //flow.AsParallel().WithDegreeOfParallelism(10).ForAll(item =>
            flow.ForEach(item =>
                {
                    requestTimestamp = requestTimestamp.AddMilliseconds(item.RequestDelay);
                    controller.IsCallAllowed(item.RequestSignature, requestTimestamp);
                });
            //actual = target.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        public static List<TestItem> GenerateTestFlow(int distinctRequestsNumber, int totalCount)
        {
            List<TestItem> flow = new List<TestItem>(totalCount);
            List<string> signatures = new List<string>(distinctRequestsNumber);

            for (int ii = 0; ii < distinctRequestsNumber; ii++)
                signatures.Add(Guid.NewGuid().ToString());

            Random rnd1=new Random(Environment.TickCount);
            for (int ii = 0; ii < totalCount; ii++)
            {
                flow.Add(new TestItem()
                {
                    RequestSignature = signatures[rnd1.Next(distinctRequestsNumber - 1)],
                    RequestDelay = rnd1.Next(1, 40)
                });
            }

            return flow;
        }

        [TestMethod()]
        public void IsCallAllowedTest_Rules()
        {
            int timeIntervalMsec = 1000;
            int maxThreshold = 5;
            LongAverageThrottlingController controller = new LongAverageThrottlingController(timeIntervalMsec, maxThreshold, 300000);
            controller.Name = "test";
            string requestSignature = Guid.NewGuid().ToString();

            DateTime requestTimestamp = DateTime.Now;//0=200
            bool result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(500);//700 (700/2=350)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(100);//800 (800/3=266)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(300);//1100 (1100/4=275)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(50);//1150 (1150/5=230)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(50);//1200 (1200/6=200)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(50);//1250 (1250/7=178)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsFalse(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(300);//1550 (1550/8=194)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsFalse(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(300);//1850 (1850/9=205)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(50);//1900 (1900/10=190)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsFalse(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(300);//2200 (2200/11=200)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void IsCallAllowedTest_Rules_Penalty()
        {
            int timeIntervalMsec = 1000;
            int maxThreshold = 5;
            LongAverageThrottlingController controller = new LongAverageThrottlingController(timeIntervalMsec, maxThreshold, 300000);
            controller.Name = "test";
            string requestSignature = Guid.NewGuid().ToString();

            DateTime requestTimestamp = DateTime.Now;//0=200
            bool result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);

            for (int ii = 0; ii < 10; ii++)
            {
                requestTimestamp = requestTimestamp.AddMilliseconds(50);//700 (700/11=64)
                result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            }

            requestTimestamp = requestTimestamp.AddMilliseconds(100);//800 (800/12=67)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsFalse(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(1050);//1850 (1850/13=142)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsFalse(result); //! should be FALSE because more than 1000 elapsed, BUT penalty

            requestTimestamp = requestTimestamp.AddMilliseconds(2000);//3850 (3850/14=275)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);
        }
    }

    public class TestItem
    {
        public string RequestSignature { get; set; }
        public int RequestDelay { get; set; }

        public TestItem()
        {
            this.RequestSignature = "";
        }
    }
}
