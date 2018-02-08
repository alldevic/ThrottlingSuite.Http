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
using System.Web;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace ThrottlingSuite.Core
{
    public interface IThrottlingScope
    {
        bool InScope(HttpContext context, bool hasTracking);
        List<ThrottlingScopeItem> Items { get; set; }
    }

    [Serializable()]
    [DataContract()]
    public class ThrottlingScope : IThrottlingScope
    {
        [DataMember(Name = "items")]
        public List<ThrottlingScopeItem> Items { get; set; }

        public ThrottlingScope()
        {
            this.Items = new List<ThrottlingScopeItem>();
        }

        public bool InScope(HttpContext context, bool hasSessionId)
        {
            ScopeItemCondition methodCondition = this.ConvertMethodToCondition(context.Request.HttpMethod);

            return this.InScope(context.Request.RawUrl, methodCondition, hasSessionId);
        }

        private bool InScope(string url, ScopeItemCondition methodCondition, bool hasSessionId)
        {
            foreach (ThrottlingScopeItem item in this.Items)
            {
                if (item.Condition == ScopeItemCondition.None
                    || (item.Condition.HasFlag(ScopeItemCondition.HasSessionId) && hasSessionId)
                    || (item.Condition.HasFlag(ScopeItemCondition.NoSessionId) && !hasSessionId)
                    || (item.Condition.HasFlag(methodCondition)))
                {//partial match
                    if (item.IsIncludeAllPath)
                    {
                        return item.Include;
                    }
                    else if (item.IsRegex)
                    {
                        if (url.StartsWith(item.Path, StringComparison.InvariantCultureIgnoreCase))
                            return item.Include;
                    }
                    else if (item.PathRegex.IsMatch(url))
                        return item.Include;
                }
            }
            return false;
        }
    }
}
