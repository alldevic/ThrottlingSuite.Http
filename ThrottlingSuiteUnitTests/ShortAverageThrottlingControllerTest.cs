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

using ThrottlingSuite.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ThrottlingSuiteUnitTests
{
    /// <summary>
    ///This is a test class for PenaltylessAverageThrottlingControllerTest and is intended
    ///to contain all PenaltylessAverageThrottlingControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ShortAverageThrottlingControllerTest
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
        ///A test for PenaltylessAverageThrottlingController Constructor
        ///</summary>
        [TestMethod()]
        public void IsCallAllowedTest()
        {
            int timeIntervalMsec = 1000; 
            int maxThreshold = 5;
            ShortAverageThrottlingController controller = new ShortAverageThrottlingController(timeIntervalMsec, maxThreshold, 300000);

            List<TestItem> flow = LongAverageThrottlingControllerTest.GenerateTestFlow(10, 1000);
            DateTime requestTimestamp = DateTime.Now;
            flow.AsParallel().WithDegreeOfParallelism(10).ForAll(item =>
            {
                requestTimestamp = requestTimestamp.AddMilliseconds(item.RequestDelay);
                controller.IsCallAllowed(item.RequestSignature, requestTimestamp);
            });
        }

        [TestMethod()]
        public void IsCallAllowedTest_Rules()
        {
            int timeIntervalMsec = 1000;
            int maxThreshold = 5;
            ShortAverageThrottlingController controller = new ShortAverageThrottlingController(timeIntervalMsec, maxThreshold, 300000);
            controller.Name = "test";
            string requestSignature = Guid.NewGuid().ToString();

            DateTime requestTimestamp = DateTime.Now;//0=1000
            bool result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result, "condition 1");

            requestTimestamp = requestTimestamp.AddMilliseconds(500);//1500 (1500/2=750)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result, "condition 2");

            requestTimestamp = requestTimestamp.AddMilliseconds(100);//1600 (1600/3=533.333)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result, "condition 3");

            requestTimestamp = requestTimestamp.AddMilliseconds(300);//1900 (1900/4=475)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result, "condition 4");

            requestTimestamp = requestTimestamp.AddMilliseconds(50);//Should recalculate time for 1st call from 1000 to 200: 1950 become 1150 (1150/5=230)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result, "condition 5");

            requestTimestamp = requestTimestamp.AddMilliseconds(50);//1200 (1200/6=200)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result, "condition 6");

            requestTimestamp = requestTimestamp.AddMilliseconds(50);//1250=1200+50 (1250/7=178)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsFalse(result, "condition 7"); //because this blocked, call should not be counted toward AVG, but the time will

            requestTimestamp = requestTimestamp.AddMilliseconds(100);//1350 (1350/7=193) (still 7 - last has not been added)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsFalse(result, "condition 8");

            requestTimestamp = requestTimestamp.AddMilliseconds(200);//1550 (1550/7=221)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result, "condition 9");

            requestTimestamp = requestTimestamp.AddMilliseconds(50);//1600 (1600/8=200)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result, "condition 10");

            requestTimestamp = requestTimestamp.AddMilliseconds(150);//1750 (1750/9=194)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsFalse(result, "condition 11");
        }

        [TestMethod()]
        public void IsCallAllowedTest_Rules_NoPenalty()
        {
            int timeIntervalMsec = 1000;
            int maxThreshold = 5;
            ShortAverageThrottlingController controller = new ShortAverageThrottlingController(timeIntervalMsec, maxThreshold, 300000);
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
            Assert.IsTrue(result); //! should be TRUE because more than 1000 elapsed

            requestTimestamp = requestTimestamp.AddMilliseconds(50);//1000+50 (1050/2=525)
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);
        }
    }
}
