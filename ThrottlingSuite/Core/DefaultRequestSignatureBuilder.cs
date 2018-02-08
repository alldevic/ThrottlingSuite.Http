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
using System.Web;

namespace ThrottlingSuite.Core
{
    public class DefaultRequestSignatureBuilder : RequestSignatureBuilderBase, IRequestSignatureBuilder
    {
        public DefaultRequestSignatureBuilder()
            : base()
        {
        }

        protected override string ComputeRequestDescription(HttpContext context)
        {
            //NOTE: access to additional parameters via this.Controller.Configuration.SignatureBuilderParams

            StringBuilder sbr = new StringBuilder();
            //resource-specific
            sbr.AppendFormat("[{0}]", context.Request.IsSecureConnection ? "HTTPS" : "HTTP");
            object tmp = context.Request.HttpMethod;
            if (tmp != null)
                sbr.AppendFormat("[{0}]", (string)tmp);
            tmp = context.Request.RawUrl;
            if (tmp != null)
            {
                //assure removing parameters to ignore from the Full URL string
                string fullUrl = (string)tmp;
                int position1 = 0, position2 = 0;
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
                sbr.AppendFormat("[{0}]", fullUrl);
            }
            if (context.Request.UrlReferrer != null)
            {
                tmp = context.Request.UrlReferrer.OriginalString;
                if (tmp != null)
                    sbr.AppendFormat("[{0}]", (string)tmp);
            }
            //user-specific
            tmp = context.Request.UserHostAddress;
            if (tmp != null)
                sbr.AppendFormat("[{0}]", (string)tmp);
            tmp = context.Request.UserHostName;
            if (tmp != null)
                sbr.AppendFormat("[{0}]", (string)tmp);
            tmp = context.Request.ServerVariables["HTTP_USER_AGENT"];
            if (tmp != null)
                sbr.AppendFormat("[{0}]", (string)tmp);
            tmp = context.Request.ServerVariables["HTTP_COOKIE"];
            if (tmp != null)
                sbr.AppendFormat("[{0}]", (string)tmp);

            if (string.Compare(context.Request.HttpMethod, "POST", true) == 0)
            {
                tmp = context.Request.Form.ToString();
                if (tmp != null)
                    sbr.AppendFormat("[{0}]", (string)tmp);
            }

            return sbr.ToString();
        }
    }
}
