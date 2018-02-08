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
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Controllers;
using System.Diagnostics;
using ThrottlingSuite.Http.Tests;

namespace ThrottlingSuiteUnitTests
{
    [TestClass()]
    public class ThrottlingServiceTest
    {
        private TestContext testContextInstance;

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

        #region Setup Throttle
        public IThrottlingService SetupThrottle()
        {
            ThrottlingServiceTestHelper helper = new ThrottlingServiceTestHelper();

            return helper.SetupThrottle();
        }
        #endregion

        private static HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, url);
            request.Content = new ByteArrayContent(new byte[] { });
            request.Properties[HttpPropertyKeys.RequestContextKey] = new HttpRequestContext() { Configuration = new System.Web.Http.HttpConfiguration() };

            return request;
        }

        [TestMethod()]
        public void IsCallAllowedTest_Allowed()
        {
            IThrottlingService throttle = SetupThrottle();

            HttpRequestMessage request = CreateRequest(HttpMethod.Get, "http://localhost/api/values");
            string blockedName;
            bool result = throttle.IsCallAllowed(request, "test", false, out blockedName);

            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void IsCallAllowedTest_Blocked()
        {
            IThrottlingService throttle = SetupThrottle();

            //1st request - should be allowed
            HttpRequestMessage request = CreateRequest(HttpMethod.Get, "http://localhost/testapi");
            string blockedName;
            bool result = throttle.IsCallAllowed(request, "test", false, out blockedName);
            Assert.IsTrue(result);

            //2nd request - should be allowed
            request = CreateRequest(HttpMethod.Get, "http://localhost/testapi");
            result = throttle.IsCallAllowed(request, "test", false, out blockedName);
            Assert.IsTrue(result);

            //3rd request - should be blocked
            request = CreateRequest(HttpMethod.Get, "http://localhost/testapi");
            result = throttle.IsCallAllowed(request, "test", false, out blockedName);
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public void IsCallAllowedTimingTest()
        {
            IThrottlingService throttle = SetupThrottle();

            string blockedName;
            bool result = false;
            Stopwatch watch = new Stopwatch();
            watch.Start();

            int counter = 0;
            for (int ii = 0; ii < 100000; ii++)
            {
                HttpRequestMessage request = CreateRequest(HttpMethod.Get, "http://localhost/api/values");
                result = throttle.IsCallAllowed(request, "test", false, out blockedName);
                if (result) counter++;
            }

            watch.Stop();
            testContextInstance.WriteLine("Total ms: {0}; OK calls: {1}", watch.ElapsedMilliseconds, counter);
            Assert.IsTrue(counter / 10 <= watch.ElapsedMilliseconds);
        }

        [TestMethod()]
        public void IsCallAllowedTimingTest2()
        {
            IThrottlingService throttle = SetupThrottle();

            string blockedName;
            bool result = false;
            Stopwatch watch = new Stopwatch();
            watch.Start();

            List<Random> randoms = new List<Random>(new[] { new Random(1000), new Random(5000), new Random(10000), new Random(35000), new Random(50000) });

            string[] urls = new[] { "http://localhost/api/users", "http://localhost/api/values", "http://localhost/api/values", "http://localhost/api/values", "http://localhost/api/content" };

            int total = randoms.AsParallel().WithDegreeOfParallelism(5).Sum(rand =>
            {
                int counter = 0;
                for (int ii = 0; ii < 20000; ii++)
                {
                    string url = urls[rand.Next(0, 4)];
                    HttpRequestMessage request = CreateRequest(HttpMethod.Get, url);
                    result = throttle.IsCallAllowed(request, "test", false, out blockedName);
                    if (result) counter++;
                }
                return counter;
            });

            watch.Stop();
            testContextInstance.WriteLine("Total ms: {0}; OK calls: {1}", watch.ElapsedMilliseconds, total);
            Assert.IsTrue(total / 10 <= watch.ElapsedMilliseconds);
        }
    }
}
