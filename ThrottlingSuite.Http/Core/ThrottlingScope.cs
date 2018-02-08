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
using System.Runtime.Serialization;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// The class providing throttling scope evaluation. 
    /// This class is paired with a throttling controller within a throttling controller instance. 
    /// It is responsible for determining whether particular HTTP request should be evaluated by specific throttling controller that it is paired with.
    /// </summary>
    [Serializable()]
    [DataContract()]
    public class ThrottlingScope : IThrottlingScope
    {
        /// <summary>
        /// Gets or sets the collection of scope items.
        /// </summary>
        [DataMember(Name = "items")]
        public List<ThrottlingScopeItem> Items { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ThrottlingScope()
        {
            this.Items = new List<ThrottlingScopeItem>();
        }

        /// <summary>
        /// Method evaluates the HTTP request and determines whether it should be processed by paired throttling controller.
        /// </summary>
        /// <param name="request">The HttpRequestMessage object to evaluate.</param>
        /// <param name="hasTracking">A boolean indicator whether particular HTTP request has client tracking information associated with the application.</param>
        /// <returns>Returns true if HTTP request is within the scope.</returns>
        public bool InScope(HttpRequestMessage request, bool hasTracking)
        {
            ScopeItemCondition methodCondition = this.ConvertMethodToCondition(request.Method.Method);

            return this.InScope(request.GetRequestVirtualPath(), methodCondition, hasTracking);
        }

        private bool InScope(string url, ScopeItemCondition methodCondition, bool hasTracking)
        {
            foreach (ThrottlingScopeItem item in this.Items)
            {
                if (item.Condition == ScopeItemCondition.None
                    || (//tracking condition has met
                        (!item.HasTrackingCondition 
                            || (item.Condition.HasFlag(ScopeItemCondition.HasTracking) && hasTracking) 
                            || (item.Condition.HasFlag(ScopeItemCondition.HasNoTracking) && !hasTracking))
                        &&
                        //HTTP method condition has met
                        (!item.HasHttpMethodCondition
                            || (item.Condition.HasFlag(methodCondition)))
                      )
                    )
                {//partial match
                    if (item.IsIncludeAllPath)
                    {
                        return item.Include;
                    }
                    else if (item.IsRegex)
                    {
                        if (item.PathRegex.IsMatch(url))
                            return item.Include;
                    }
                    else if (url.StartsWith(item.Path, StringComparison.InvariantCultureIgnoreCase))
                        return item.Include;
                }
            }
            return false;
        }
    }
}
