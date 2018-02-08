using System;
using System.Net.Http;

namespace ThrottlingSuite.Core
{
    public static class HttpRequestMessageExtensions
    {
        private const string HttpContext = "MS_HttpContext";
        private const string RemoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";
        private const string OwinContext = "MS_OwinContext";

        /// <summary>
        /// Obtains client IP address for HTTP request using different method depending on the current runtime.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>Returns a string representing client IP address.</returns>
        internal static string GetClientIpAddress(this HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            // Web-hosting. Needs reference to System.Web.dll
            if (request.Properties.ContainsKey(HttpContext))
            {
                dynamic ctx = request.Properties[HttpContext];
                if (ctx != null)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            // Self-hosting. Needs reference to System.ServiceModel.dll. 
            if (request.Properties.ContainsKey(RemoteEndpointMessage))
            {
                dynamic remoteEndpoint = request.Properties[RemoteEndpointMessage];
                if (remoteEndpoint != null)
                {
                    return remoteEndpoint.Address;
                }
            }

            // Self-hosting using Owin. Needs reference to Microsoft.Owin.dll. 
            if (request.Properties.ContainsKey(OwinContext))
            {
                dynamic owinContext = request.Properties[OwinContext];
                if (owinContext != null)
                {
                    return owinContext.Request.RemoteIpAddress;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns virtual path for HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>Returns a string. It would not have leading "/".</returns>
        internal static string GetRequestVirtualPath(this HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            const string RequestVirtualUrlPathKey = "TS_RequestVirtualUrlPath";
            if (request.Properties.ContainsKey(RequestVirtualUrlPathKey))
                return (string)request.Properties[RequestVirtualUrlPathKey];

            string virtualPathRoot = request.GetRequestContext().VirtualPathRoot;
            if (string.IsNullOrWhiteSpace(virtualPathRoot))
            {
                virtualPathRoot = "/";
            }
            else if (!virtualPathRoot.EndsWith("/", StringComparison.Ordinal))
            {
                virtualPathRoot += "/";
            }

            //The virtual path root should always end with a "/" to ensure accurate substring from the absolute path.
            string virtualPath = request.RequestUri.AbsolutePath.Substring(virtualPathRoot.Length);
            request.Properties.Add(RequestVirtualUrlPathKey, virtualPath);

            return virtualPath;
        }

        internal static DateTime GetTimestamp(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(ThrottlingConfiguration.RequestTimestampPropertyName))
                return (DateTime)request.Properties[ThrottlingConfiguration.RequestTimestampPropertyName];

            DateTime stamp = DateTime.Now;
            request.Properties[ThrottlingConfiguration.RequestTimestampPropertyName] = stamp;

            return stamp;
        }
    }
}
