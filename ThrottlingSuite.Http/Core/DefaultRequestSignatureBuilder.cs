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
using System.Text;
using System.Net.Http;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// Class implements IRequestSignatureBuilder interface and provides HTTP request signature building functionality.
    /// </summary>
    public class DefaultRequestSignatureBuilder : RequestSignatureBuilderBase, IRequestSignatureBuilder
    {
        /// <summary>
        /// The configuration object used by the class.
        /// </summary>
        public DefaultRequestSignatureBuilder()
            : base()
        {
        }

        /// <summary>
        /// Computes the HTTP request long description.
        /// </summary>
        /// <param name="request">The HTTP request object.</param>
        /// <returns>Returns a description as a string.</returns>
        protected override string ComputeRequestDescription(HttpRequestMessage request)
        {
            //NOTE: access to additional parameters via this.Controller.Configuration.SignatureBuilderParams

            StringBuilder sbr = new StringBuilder();
            object tmp;
            //resource-specific
            if (!Configuration.SignatureBuilderParams.UseInstanceUrl)
            {
                ComputeFromUrl(request.RequestUri.PathAndQuery, ref sbr);
            }

            //user-specific tracking
            if (Configuration.SignatureBuilderParams.IgnoreClientIpAddress)
            {
                tmp = request.GetClientIpAddress();
                if (tmp != null)
                    sbr.Append($"[{(string)tmp}]");
            }
            if (Configuration.SignatureBuilderParams.EnableClientTracking)
            {
                tmp = request.Headers.GetCookies(ThrottlingConfiguration.CookieTrackingName);
                if (tmp != null)
                    sbr.Append($"[{string.Join("", tmp)}]");
            }

            return sbr.ToString();
        }

        private void ComputeFromUrl(string pathAndQuery, ref StringBuilder sbr)
        {
            // - URI
            if (!string.IsNullOrWhiteSpace(pathAndQuery))
            {
                //assure removing parameters to ignore from the Full URL string
                var fullUrl = pathAndQuery;
                int position1, position2;
                if (Configuration.SignatureBuilderParams.IgnoreAllQueryStringParameters)
                {
                    position1 = fullUrl.IndexOf("?", StringComparison.InvariantCultureIgnoreCase);
                    if (position1 > 0)
                        fullUrl = fullUrl.Substring(0, position1);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(fullUrl) && Configuration.SignatureBuilderParams.IgnoreParameters.Count > 0)
                    {
                        foreach (string ignoreParam in Configuration.SignatureBuilderParams.IgnoreParameters)
                        {
                            position1 = fullUrl.IndexOf(ignoreParam, StringComparison.InvariantCultureIgnoreCase);
                            if (position1 >= 0)
                            {
                                position2 = fullUrl.IndexOf("&", position1, StringComparison.InvariantCultureIgnoreCase);
                                if (position2 >= 0)
                                    fullUrl = fullUrl.Remove(position1, position2 - position1);
                                else
                                    fullUrl = fullUrl.Remove(position1);
                                if (fullUrl.EndsWith("&"))
                                    fullUrl = fullUrl.Remove(fullUrl.Length - 1, 1);
                            }
                        }
                    }
                }
                sbr.Append($"[{fullUrl}]");
            }
        }
    }
}
