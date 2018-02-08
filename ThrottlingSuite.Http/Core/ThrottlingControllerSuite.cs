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
using System.Xml;
using System.Net.Http;

namespace ThrottlingSuite.Core
{
    public partial class ThrottlingControllerSuite : IThrottlingService
    {
        /// <summary>
        /// Method evaluates whether the call (HTTP request) is allowed to go through or should be blocked.
        /// </summary>
        /// <param name="request">The HTTP request to validate.</param>
        /// <param name="requestSignature">Request signature as a string.</param>
        /// <param name="hasTracking">A boolean indicator whether particular HTTP request has client tracking information associated with the application.</param>
        /// <param name="blockedInstanceName">A controller instance name that blocked the call, in case it should be blocked.</param>
        /// <returns>Returns boolean value indicating whether the call (HTTP request) is allowed to go through.</returns>
        public bool IsCallAllowed(HttpRequestMessage request, string requestSignature, bool hasTracking, out string blockedInstanceName)
        {
            blockedInstanceName = "";
            DateTime requestTimestamp = request.GetTimestamp();
            string requestSignatureInScope = string.Empty;
            for (int ii = 0; ii < this.Instances.Count; ii++)
            {
                if (this.Instances[ii].Scope.InScope(request, hasTracking))
                {
                    requestSignatureInScope = string.Join("-", this.Instances[ii].Name, requestSignature);
                    if (!this.Instances[ii].Controller.IsCallAllowed(requestSignatureInScope, requestTimestamp))
                    {
                        blockedInstanceName = this.Instances[ii].Name;
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
