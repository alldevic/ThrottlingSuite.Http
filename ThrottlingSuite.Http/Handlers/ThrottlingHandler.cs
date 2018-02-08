#region "Copyright (C) Lenny Granovsky. 2012-2014"
//
//                Copyright (C) Lenny Granovsky. 2012-2014. 
//
//   Permission is hereby granted, free of charge, to any person obtaining a copy of 
//   this software and associated documentation files (the "Software"), to deal in 
//   the Software without restriction, including without limitation the rights to 
//   use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
//   of the Software, and to permit persons to whom the Software is furnished to 
//   do so, subject to the following conditions:

//   The above copyright notice and this permission notice shall be included in 
//   all copies or substantial portions of the Software.

//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//   INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
//   PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
//   HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
//   OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Tracing;
using ThrottlingSuite.Core;

namespace ThrottlingSuite.Http.Handlers
{
    /// <summary>
    /// Class represents the WebApi handler providing HTTP request throttling.
    /// </summary>
    public class ThrottlingHandler : DelegatingHandler
    {
        private IThrottlingService throttlingService;
        private IRequestSignatureBuilder RequestSignatureBuilder;

        /// <summary>
        /// Constructor that requires the instance of a class that implements IThrottlingService. 
        /// </summary>
        /// <param name="throttlingService">The instance of a class that implements IThrottlingService.</param>
        public ThrottlingHandler(IThrottlingService throttlingService)
        {
            if (throttlingService == null)
                throw new ArgumentNullException("throttlingService");

            this.throttlingService = throttlingService;
            this.EnsureSignatureBuilder(this.throttlingService.Configuration);
        }

        private void EnsureSignatureBuilder(ThrottlingConfiguration configuration)
        {
            this.RequestSignatureBuilder = (IRequestSignatureBuilder)Activator.CreateInstance(configuration.RequestSignatureBuilderType);
            this.RequestSignatureBuilder.Init(configuration);
        }

        /// <summary>
        /// Method returns the instance of the throttling service (controller suite) used by this ThrottlingHandler.
        /// </summary>
        /// <returns>Returns an instance of class that implements IThrottlingService interface.</returns>
        public IThrottlingService GetThrottlingService()
        {
            return this.throttlingService;
        }

        /// <summary>
        /// Throttles an HTTP request and either rejects the request with HTTP Status 429 "Too many requests", 
        /// or sends HTTP request to the inner handler as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request to send.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>Returns a Task&lt;HttpResponseMessage&gt; type.</returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            double recommendedDelay = 0;
            if (this.EvaluateRequest(request, out recommendedDelay))
            {
                // Call the inner handler.
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

                this.EnsureTrackableResponse(request, response);

                return response;
            }
            
            //return 429 Too Many Requests
            HttpResponseMessage errorResponse = request.CreateResponse((HttpStatusCode)429); 
            errorResponse.ReasonPhrase = "Too Many Requests";

            recommendedDelay = Math.Ceiling(recommendedDelay / 1000);
            errorResponse.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(new TimeSpan(0, 0, Convert.ToInt32(recommendedDelay)));
            return errorResponse;
        }

        private bool EvaluateRequest(HttpRequestMessage request, out double recommendedDelay)
        {
            recommendedDelay = 0;
            if (request != null)
            {
#if TRACE
                ITraceWriter trace = null;
                Stopwatch watch = null;
#endif
                string requestSignature = "";
                try
                {
                    request.GetTimestamp(); //ensure timestamp;
#if TRACE
                    trace = request.GetConfiguration().Services.GetTraceWriter();
#endif
                    if (!this.throttlingService.Configuration.Enabled)
                    {
#if TRACE
                        if (trace != null)
                            trace.Info(request, "WebApi.Throttling", "Throttling functionality is currently disabled.");
#endif
                        return true;
                    }
#if TRACE
                    watch = new Stopwatch();
                    watch.Start();
#endif
                    //compute request signature
                    requestSignature = this.RequestSignatureBuilder.ComputeRequestSignature(request);

                    bool hasTracking = HasTrackingCookie(request);
                    //verify if call can go through
                    string blockedInstanceName = "";
                    if (!this.throttlingService.IsCallAllowed(request, requestSignature, hasTracking, out blockedInstanceName))
                    {
                        IThrottlingController blockedBy = this.throttlingService.GetControllerByName(blockedInstanceName);
                        recommendedDelay = blockedBy.TimeIntervalMsec / blockedBy.MaxThreshold;
                        if (this.throttlingService.Configuration.LogOnly)
                        {
#if TRACE
                            if (trace != null)
                                trace.Info(request, "WebApi.Throttling", "Request: {0} has been blocked by {1}.", requestSignature, blockedInstanceName);
                            //continue to the end and return TRUE if "log only" mode.
#endif
                        }
                        else
                            return false;
                    }
                }
                finally
                {
#if TRACE
                    if (trace != null && watch != null)
                    {
                        watch.Stop();
                        trace.Info(request, "WebApi.Throttling", "Request: {0}; DuplicatedCallsHandler complete in {1} ms.", requestSignature, watch.Elapsed.TotalMilliseconds.ToString());
                    }
#endif
                }
            }

            return true;
        }

        private void EnsureTrackableResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            if (this.throttlingService.Configuration.SignatureBuilderParams.EnableClientTracking)
            {
                if (request != null && response != null)
                {
                    //if no ASP.NET_SessionId cookies exists, add TCM (throttling control marker) value to mark all requests from specific client.
                    bool hasTracking = HasTrackingCookie(request);
                    if (!hasTracking)
                        response.Headers.AddCookies(new[] { new System.Net.Http.Headers.CookieHeaderValue(ThrottlingConfiguration.CookieTrackingName, Guid.NewGuid().ToString().Replace("-", "").ToLower()) });
                }
            }
        }

        private bool HasTrackingCookie(HttpRequestMessage request)
        {
            if (this.throttlingService.Configuration.SignatureBuilderParams.EnableClientTracking)
            {
                return (request.Headers.GetCookies(ThrottlingConfiguration.CookieTrackingName) != null);
            }
            return false;
        }

        /// <summary>
        /// Disposes resources used by the object, including all the throttling controllers it uses.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (this.throttlingService != null)
                (this.throttlingService as IDisposable).Dispose();
            base.Dispose(disposing);
        }
    }
}
