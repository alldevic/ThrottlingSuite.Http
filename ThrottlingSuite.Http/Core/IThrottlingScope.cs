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

using System.Collections.Generic;
using System.Net.Http;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// An interface that shell be implemented by a class providing throttling scope evaluation. 
    /// The class implemented this interface is paired with a throttling controller within a throttling controller instance. 
    /// Such class is responsible for determining whether particular HTTP request should be evaluated by specific throttling controller that it is paired with.
    /// </summary>
    public interface IThrottlingScope
    {
        /// <summary>
        /// Method evaluates the HTTP request and determines whether it should be processed by paired throttling controller.
        /// </summary>
        /// <param name="request">The HttpRequestMessage object to evaluate.</param>
        /// <param name="hasTracking">A boolean indicator whether particular HTTP request has client tracking information associated with the application.</param>
        /// <returns>Returns true if HTTP request is within the scope.</returns>
        bool InScope(HttpRequestMessage request, bool hasTracking);
        /// <summary>
        /// Gets or sets the collection of scope items.
        /// </summary>
        List<ThrottlingScopeItem> Items { get; set; }
    }
}
