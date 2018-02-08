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
    /// Enumeration for special condition matching an HTTP request.
    /// </summary>
    [Serializable()]
    [Flags]
    public enum ScopeItemCondition
    {
        /// <summary>
        /// No special conditions are set.
        /// </summary>
        None,
        /// <summary>
        /// Not in use.
        /// </summary>
        [Obsolete("Use HasTracking instead.")]
        HasSessionId,
        /// <summary>
        /// Not in use.
        /// </summary>
        [Obsolete("Use HasNoTracking instead.")]
        NoSessionId,
        /// <summary>
        /// A request should have client tracking information.
        /// </summary>
        HasTracking,
        /// <summary>
        /// A request should not have client tracking information.
        /// </summary>
        HasNoTracking,
        /// <summary>
        /// An HTTP method for request should be GET.
        /// </summary>
        HttpGet,
        /// <summary>
        /// An HTTP method for request should be POST.
        /// </summary>
        HttpPost,
        /// <summary>
        /// An HTTP method for request should be PUT.
        /// </summary>
        HttpPut,
        /// <summary>
        /// An HTTP method for request should be DELETE.
        /// </summary>
        HttpDelete,
        /// <summary>
        /// An HTTP method for request should be OPTIONS.
        /// </summary>
        HttpOptions,
        /// <summary>
        /// An HTTP method for request should be TRACE.
        /// </summary>
        HttpTrace,
        /// <summary>
        /// An HTTP method for request should be PATCH.
        /// </summary>
        HttpPatch,
        /// <summary>
        /// An HTTP method for request should be HEAD.
        /// </summary>
        HttpHead
    }
}
