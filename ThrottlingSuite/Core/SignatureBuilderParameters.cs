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

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// Class provides collection of configuration parameters for building HTTP request signature.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2229:ImplementSerializationConstructors"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly")]
    [Serializable()]
    public class SignatureBuilderParameters : Dictionary<string, string>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SignatureBuilderParameters()
            : base(StringComparer.CurrentCultureIgnoreCase)
        {
            this.IgnoreParameters = new List<string>();
        }

        /// <summary>
        /// Gets the list of parameters to be ignored.
        /// </summary>
        /// <remarks>You may change this parameter during the runtime.</remarks>
        public List<string> IgnoreParameters { get; private set; }

        /// <summary>
        /// Gets a boolean indicator whether all query string parameters should be ignored.
        /// </summary>
        /// <remarks>You may change this parameter during the runtime.</remarks>
        public bool IgnoreAllQueryStringParameters
        {
            get
            {
                if (!this.ContainsKey("ignoreAllQueryStringParameters"))
                    return false;
                return string.Compare(this["ignoreAllQueryStringParameters"], "true", true) == 0;
            }
            set
            {
                if (this.ContainsKey("ignoreAllQueryStringParameters"))
                    this["ignoreAllQueryStringParameters"] = value.ToString();
                else
                    this.Add("ignoreAllQueryStringParameters", value.ToString());
            }
        }

        /// <summary>
        /// Gets a boolean indicator whether client IP address should be ignored.
        /// </summary>
        /// <remarks>You may change this parameter during the runtime.</remarks>
        public bool IgnoreClientIpAddress
        {
            get
            {
                if (!this.ContainsKey("ignoreClientIp"))
                    return false;
                return string.Compare(this["ignoreClientIp"], "true", true) == 0;
            }
            set
            {
                if (this.ContainsKey("ignoreClientIp"))
                    this["ignoreClientIp"] = value.ToString();
                else
                    this.Add("ignoreClientIp", value.ToString());
            }
        }

        /// <summary>
        /// Gets a boolean indicator whether client tracking should be enabled via cookies.
        /// </summary>
        /// <remarks>You may change this parameter during the runtime.</remarks>
        public bool EnableClientTracking
        {
            get
            {
                if (!this.ContainsKey("enableClientTracking"))
                    return false;
                return string.Compare(this["enableClientTracking"], "true", true) == 0;
            }
            set
            {
                if (this.ContainsKey("enableClientTracking"))
                    this["enableClientTracking"] = value.ToString();
                else
                    this.Add("enableClientTracking", value.ToString());
            }
        }

        /// <summary>
        /// Gets a boolean indicator whether the actual HTTP request URL is being substituted with controller instance scope. 
        /// This allows to handle throttling threshold per instance combining multiple URLs from different API calls.
        /// </summary>
        /// <remarks>You may change this parameter during the runtime.</remarks>
        public bool UseInstanceUrl
        {
            get
            {
                if (!this.ContainsKey("useInstanceUrl"))
                    return false;
                return string.Compare(this["useInstanceUrl"], "true", true) == 0;
            }
            set
            {
                if (this.ContainsKey("useInstanceUrl"))
                    this["useInstanceUrl"] = value.ToString();
                else
                    this.Add("useInstanceUrl", value.ToString());
            }
        }
    }
}
