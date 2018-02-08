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
using System.Collections.Generic;
using System.Linq;

namespace ThrottlingSuiteUnitTests
{
    /// <summary>
    ///This is a test class for LinearThrottlingControllerTest and is intended
    ///to contain all LinearThrottlingControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LinearThrottlingControllerTest
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
            LinearThrottlingController controller = new LinearThrottlingController(timeIntervalMsec, maxThreshold, 300000);
            controller.Name = "test";

            List<TestItem> flow = GenerateTestFlow(10, 1000);
            DateTime requestTimestamp = DateTime.Now;
            flow.AsParallel().WithDegreeOfParallelism(10).ForAll(item =>
            {
                requestTimestamp = requestTimestamp.AddMilliseconds(item.RequestDelay);
                controller.IsCallAllowed(item.RequestSignature, requestTimestamp);
            });
        }

        public static List<TestItem> GenerateTestFlow(int distinctRequestsNumber, int totalCount)
        {
            List<TestItem> flow = new List<TestItem>(totalCount);
            List<string> signatures = new List<string>(distinctRequestsNumber);

            for (int ii = 0; ii < distinctRequestsNumber; ii++)
                signatures.Add(Guid.NewGuid().ToString());

            Random rnd1 = new Random(Environment.TickCount);
            for (int ii = 0; ii < totalCount; ii++)
            {
                flow.Add(new TestItem()
                {
                    RequestSignature = signatures[rnd1.Next(distinctRequestsNumber - 1)],
                    RequestDelay = rnd1.Next(1, 400)
                });
            }

            return flow;
        }

        /// <summary>
        ///A test for IsCallAllowed
        ///</summary>
        [TestMethod()]
        public void IsCallAllowedTest_Rules()
        {
            int timeIntervalMsec = 1000;
            int maxThreshold = 5;
            LinearThrottlingController controller = new LinearThrottlingController(timeIntervalMsec, maxThreshold, 300000);
            controller.Name = "test";
            string requestSignature = Guid.NewGuid().ToString();

            DateTime requestTimestamp = DateTime.Now;
            bool result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(500);
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(130);
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsFalse(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(300);
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsTrue(result);

            requestTimestamp = requestTimestamp.AddMilliseconds(150);
            result = controller.IsCallAllowed(requestSignature, requestTimestamp);
            Assert.IsFalse(result);
        }
    }
}
