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
using System.Linq;
using System.Text;
using System.Security.Cryptography;
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
            object tmp = null;
            //resource-specific
            if (!this.Configuration.SignatureBuilderParams.UseInstanceUrl)
            {
                this.ComputeFromUrl(request.RequestUri.PathAndQuery, ref sbr);
            }

            //user-specific tracking
            if (this.Configuration.SignatureBuilderParams.IgnoreClientIpAddress)
            {
                tmp = request.GetClientIpAddress();
                if (tmp != null)
                    sbr.AppendFormat("[{0}]", (string)tmp);
            }
            if (this.Configuration.SignatureBuilderParams.EnableClientTracking)
            {
                tmp = request.Headers.GetCookies(ThrottlingConfiguration.CookieTrackingName);
                if (tmp != null)
                    sbr.AppendFormat("[{0}]", string.Join("", tmp));
            }

            return sbr.ToString();
        }

        private void ComputeFromUrl(string pathAndQuery, ref StringBuilder sbr)
        {
            // - URI
            if (!string.IsNullOrWhiteSpace(pathAndQuery))
            {
                //assure removing parameters to ignore from the Full URL string
                string fullUrl = (string)pathAndQuery;
                int position1 = 0, position2 = 0;
                if (this.Configuration.SignatureBuilderParams.IgnoreAllQueryStringParameters)
                {
                    position1 = fullUrl.IndexOf("?", StringComparison.InvariantCultureIgnoreCase);
                    if (position1 > 0)
                        fullUrl = fullUrl.Substring(0, position1);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(fullUrl) && this.Configuration.SignatureBuilderParams.IgnoreParameters.Count > 0)
                    {
                        foreach (string ignoreParam in this.Configuration.SignatureBuilderParams.IgnoreParameters)
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
                sbr.AppendFormat("[{0}]", fullUrl);
            }
        }
    }
}
