using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlingSuite.Http.Tests
{
    public class InnerHandlerMock : DelegatingHandler
    {
        private HttpResponseMessage responseToReturn;

        public void AssertResponse(HttpResponseMessage response)
        {
            this.responseToReturn = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.responseToReturn == null)
                return base.SendAsync(request, cancellationToken);

            return Task.Factory.StartNew(() => this.responseToReturn, cancellationToken);
        }
    }
}
