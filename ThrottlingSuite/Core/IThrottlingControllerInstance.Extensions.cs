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
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// Implements extensions for include in scope and exclude from scope.
    /// </summary>
    public static class IThrottlingControllerInstanceExtensions
    {
        /// <summary>
        /// Includes HTTP request URL path into the throttling controller scope.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="path">Path to include.</param>
        /// <returns>Returns the throttling controller instance.</returns>
        public static IThrottlingControllerInstance IncludeInScope(this IThrottlingControllerInstance instance, string path)
        {
            instance.Scope.Include(path, ScopeItemCondition.None);
            return instance;
        }

        /// <summary>
        /// Includes HTTP request URL path with specified condition into the throttling controller scope.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="path">Path to include.</param>
        /// <param name="condition">Condition to apply to the request.</param>
        /// <returns>Returns the throttling controller instance.</returns>
        public static IThrottlingControllerInstance IncludeInScope(this IThrottlingControllerInstance instance, string path, ScopeItemCondition condition)
        {
            instance.Scope.Include(path, condition);
            return instance;
        }

        /// <summary>
        /// Includes HTTP request URL path Regular Expression with specified condition into the throttling controller scope.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="pathRegex">Path Regular Expression to include.</param>
        /// <returns>Returns the throttling controller instance.</returns>
        public static IThrottlingControllerInstance IncludeInScope(this IThrottlingControllerInstance instance, Regex pathRegex)
        {
            instance.Scope.Include(pathRegex, ScopeItemCondition.None);
            return instance;
        }

        /// <summary>
        /// Includes HTTP request URL path Regular Expression with specified condition into the throttling controller scope.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="pathRegex">Path Regular Expression to include.</param>
        /// <param name="condition">Condition to apply to the request.</param>
        /// <returns>Returns the throttling controller instance.</returns>
        public static IThrottlingControllerInstance IncludeInScope(this IThrottlingControllerInstance instance, Regex pathRegex, ScopeItemCondition condition)
        {
            instance.Scope.Include(pathRegex, condition);
            return instance;
        }

        /// <summary>
        /// Excludes HTTP request URL path with specified condition from the throttling controller scope.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="path">Path to exclude.</param>
        /// <returns>Returns the throttling controller instance.</returns>
        public static IThrottlingControllerInstance ExcludeFromScope(this IThrottlingControllerInstance instance, string path)
        {
            instance.Scope.Exclude(path, ScopeItemCondition.None);
            return instance;
        }

        /// <summary>
        /// Excludes HTTP request URL path with specified condition from the throttling controller scope.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="path">Path to exclude.</param>
        /// <param name="condition">Condition to apply to the request.</param>
        /// <returns>Returns the throttling controller instance.</returns>
        public static IThrottlingControllerInstance ExcludeFromScope(this IThrottlingControllerInstance instance, string path, ScopeItemCondition condition)
        {
            instance.Scope.Exclude(path, condition);
            return instance;
        }

        /// <summary>
        /// Excludes HTTP request URL path Regular Expression with specified condition from the throttling controller scope.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="pathRegex">Path Regular Expression to exclude.</param>
        /// <returns>Returns the throttling controller instance.</returns>
        public static IThrottlingControllerInstance ExcludeFromScope(this IThrottlingControllerInstance instance, Regex pathRegex)
        {
            instance.Scope.Exclude(pathRegex, ScopeItemCondition.None);
            return instance;
        }

        /// <summary>
        /// Excludes HTTP request URL path Regular Expression with specified condition from the throttling controller scope.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="pathRegex">Path Regular Expression to exclude.</param>
        /// <param name="condition">Condition to apply to the request.</param>
        /// <returns>Returns the throttling controller instance.</returns>
        public static IThrottlingControllerInstance ExcludeFromScope(this IThrottlingControllerInstance instance, Regex pathRegex, ScopeItemCondition condition)
        {
            instance.Scope.Exclude(pathRegex, condition);
            return instance;
        }
    }
}
