using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ThrottlingSuite.Http.Tests
{
    public class InnerHandlerMock : DelegatingHandler
    {
        private HttpResponseMessage responseToReturn;

        public void AssertResponse(HttpResponseMessage response)
        {
            responseToReturn = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (responseToReturn == null)
                return base.SendAsync(request, cancellationToken);

            return Task.Factory.StartNew(() => responseToReturn, cancellationToken);
        }
    }
}
