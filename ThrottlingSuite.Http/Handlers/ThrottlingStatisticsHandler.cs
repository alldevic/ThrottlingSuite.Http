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
using ThrottlingSuite.Core;

namespace ThrottlingSuite.Http.Handlers
{
    /// <summary>
    /// Class represents the WebApi handler providing statistics for current throttling suite.
    /// </summary>
    public class ThrottlingStatisticsHandler : DelegatingHandler
    {
        private IThrottlingService throttlingService;

        /// <summary>
        /// The constructor. A valid reference to the throttling service must be provided.
        /// </summary>
        /// <param name="throttlingService">The instance of a class that implements IThrottlingService.</param>
        public ThrottlingStatisticsHandler(IThrottlingService throttlingService)
            : base()
        {
            if (throttlingService == null)
                throw new ArgumentNullException("throttlingService");

            this.throttlingService = throttlingService;
        }

        /// <summary>
        /// Handles an HTTP request as an asynchronous operation and provides HttpResponseMessage with throttling statistics data.
        /// </summary>
        /// <param name="request">The HTTP request to handle.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>Returns a Task&lt;HttpResponseMessage&gt; type.</returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = this.ProcessRequest(request);

            return await Task.Run(() => response);
        }

        private HttpResponseMessage ProcessRequest(HttpRequestMessage request)
        {
            ThrottlingStatisticsAdapter adapter = new ThrottlingStatisticsAdapter();
            ThrottlingStatisticsData data = new ThrottlingStatisticsData();
            if (this.throttlingService != null)
            {
                //collect data
                data = adapter.CollectStatistics(this.throttlingService, null);
            }
            else
                data.Message = "The Throttling Controller Suite is not yet created.";

            return request.CreateResponse<ThrottlingStatisticsData>(data);
        }
    }
}
