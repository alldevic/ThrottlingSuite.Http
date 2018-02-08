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
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// Class contains the rules for a throttling scope item.
    /// </summary>
    [Serializable()]
    [DataContract()]
    public class ThrottlingScopeItem
    {
        private string path;
        private bool isIncludeAllPath;
        /// <summary>
        /// Gets or sets the path root the HTTP request URL begins with.
        /// </summary>
        [DataMember(Name = "path", Order = 1)]
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                isIncludeAllPath = (value == "*" || value == "*.*");
                path = value;
            }
        }
        /// <summary>
        /// Gets a boolean indication whether the Path contains a substitute for "All".
        /// </summary>
        public bool IsIncludeAllPath
        {
            get
            {
                return isIncludeAllPath;
            }
        }
        /// <summary>
        /// Gets or sets the path Regex that suppose to match the request URL.
        /// </summary>
        public Regex PathRegex { get; set; }
        /// <summary>
        /// Gets or sets boolean indicator whether HTTP request that matches this scope item should be inclusive (true) or exclusive toward the throttling scope overall.
        /// </summary>
        [DataMember(Name = "include", Order = 3)]
        public bool Include { get; set; }

        /// <summary>
        /// Gets or sets boolean indicator whether this scope item is a Regex match.
        /// </summary>
        public bool IsRegex
        {
            get { return PathRegex != null; }
        }

        private ScopeItemCondition condition;
        /// <summary>
        /// Gets or sets special conditions for matching an HTTP request.
        /// </summary>
        [DataMember(Name = "condition", Order = 2)]
        public ScopeItemCondition Condition
        {
            get { return condition; }
            set
            {
                condition = value;
                if (value.HasFlag(ScopeItemCondition.HasSessionId))
                    condition = condition | ScopeItemCondition.HasTracking;
                if (value.HasFlag(ScopeItemCondition.NoSessionId))
                    condition = condition | ScopeItemCondition.HasNoTracking;

                HasTrackingCondition = value.HasFlag(ScopeItemCondition.HasNoTracking)
                    || value.HasFlag(ScopeItemCondition.HasTracking);

                HasHttpMethodCondition = value.HasFlag(ScopeItemCondition.HttpDelete)
                    || value.HasFlag(ScopeItemCondition.HttpGet)
                    || value.HasFlag(ScopeItemCondition.HttpOptions)
                    || value.HasFlag(ScopeItemCondition.HttpPatch)
                    || value.HasFlag(ScopeItemCondition.HttpPost)
                    || value.HasFlag(ScopeItemCondition.HttpPut)
                    || value.HasFlag(ScopeItemCondition.HttpTrace);
            }
        }

        /// <summary>
        /// Gets boolean indicator whether at least one of special conditions is a particular HTTP method.
        /// </summary>
        public bool HasHttpMethodCondition { get; private set; }
        /// <summary>
        /// Gets boolean indicator whether at least one of special conditions is a client tracking information within an HTTP request.
        /// </summary>
        public bool HasTrackingCondition { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ThrottlingScopeItem()
        {
            Path = "";
            Condition = ScopeItemCondition.None;
            Include = true;
        }
    }
}
