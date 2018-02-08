using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThrottlingSuite.Core;
using ThrottlingSuite.Http.Handlers;

namespace ThrottlingSuite.Http.Tests
{
    [TestClass()]
    public class ThrottlingHandlerTest
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
        public ThrottlingHandler SetupThrottleHandler()
        {
            ThrottlingServiceTestHelper helper = new ThrottlingServiceTestHelper();

            IThrottlingService throttle = helper.SetupThrottle();

            ThrottlingHandler handler = new ThrottlingHandler(throttle);

            InnerHandlerMock innerHandler = new InnerHandlerMock();
            innerHandler.AssertResponse(new HttpResponseMessage(HttpStatusCode.OK));
            handler.InnerHandler = innerHandler;

            return handler;
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
            ThrottlingHandler handler = this.SetupThrottleHandler();
            HttpMessageInvoker invoker = new HttpMessageInvoker(handler);

            HttpRequestMessage request = CreateRequest(HttpMethod.Get, "http://localhost/api/values");
            HttpResponseMessage response = invoker.SendAsync(request, new CancellationToken(false)).Result;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod()]
        public void IsCallAllowedTest_Blocked()
        {
            ThrottlingHandler handler = this.SetupThrottleHandler();
            HttpMessageInvoker invoker = new HttpMessageInvoker(handler);

            //1st request - should be allowed
            HttpRequestMessage request = CreateRequest(HttpMethod.Get, "http://localhost/testapi");
            HttpResponseMessage response = invoker.SendAsync(request, new CancellationToken(false)).Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(response.Headers.RetryAfter);

            //2nd request - should be allowed
            request = CreateRequest(HttpMethod.Get, "http://localhost/testapi");
            response = invoker.SendAsync(request, new CancellationToken(false)).Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(response.Headers.RetryAfter);

            //3rd request - should be blocked
            request = CreateRequest(HttpMethod.Get, "http://localhost/testapi");
            response = invoker.SendAsync(request, new CancellationToken(false)).Result;
            Assert.AreEqual((HttpStatusCode)429, response.StatusCode);
            Assert.AreEqual("Too Many Requests", response.ReasonPhrase);
            Assert.IsNotNull(response.Headers.RetryAfter);
            Assert.IsNotNull(response.Headers.RetryAfter.Delta);
            Assert.AreEqual(1000, response.Headers.RetryAfter.Delta.Value.TotalMilliseconds);
        }

        [TestMethod()]
        public void IsCallAllowedTimingTest()
        {
            ThrottlingHandler handler = this.SetupThrottleHandler();
            HttpMessageInvoker invoker = new HttpMessageInvoker(handler);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            int counter = 0;
            for (int ii = 0; ii < 100000; ii++)
            {
                HttpRequestMessage request = CreateRequest(HttpMethod.Get, "http://localhost/api/values");
                HttpResponseMessage response = invoker.SendAsync(request, new CancellationToken(false)).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                    counter++;
            }

            watch.Stop();
            this.testContextInstance.WriteLine("Total ms: {0}; OK calls: {1}", watch.ElapsedMilliseconds, counter);
            Assert.IsTrue(counter / 10 <= watch.ElapsedMilliseconds);
        }
    }
}
