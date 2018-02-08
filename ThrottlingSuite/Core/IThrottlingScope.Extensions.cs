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
using System.Text.RegularExpressions;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// Implements extensions for include and exclude scope items.
    /// </summary>
    public static class IThrottlingScopeExtensions
    {
        /// <summary>
        /// Includes HTTP request URL path into the throttling controller scope.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="path">Path to include.</param>
        /// <returns>Returns the throttling scope.</returns>
        public static IThrottlingScope Include(this IThrottlingScope scope, string path)
        {
            return scope.Include(path, ScopeItemCondition.None);
        }

        /// <summary>
        /// Includes HTTP request URL path with specified condition into the throttling controller scope.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="path">Path to include.</param>
        /// <param name="condition">Condition to apply to the request.</param>
        /// <returns>Returns the throttling scope.</returns>
        public static IThrottlingScope Include(this IThrottlingScope scope, string path, ScopeItemCondition condition)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            path = AdjustToVirtualPath(path);

            scope.AddAndOptimizeOrder(new ThrottlingScopeItem()
            {
                Include = true,
                Path = path,
                Condition = condition
            });

            return scope;
        }

        /// <summary>
        /// Includes HTTP request URL path Regular Expression with specified condition into the throttling controller scope.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="pathRegex">Path Regular Expression to include.</param>
        /// <returns>Returns the throttling scope.</returns>
        public static IThrottlingScope Include(this IThrottlingScope scope, Regex pathRegex)
        {
            return scope.Include(pathRegex, ScopeItemCondition.None);
        }

        /// <summary>
        /// Includes HTTP request URL path Regular Expression with specified condition into the throttling controller scope.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="pathRegex">Path Regular Expression to include.</param>
        /// <param name="condition">Condition to apply to the request.</param>
        /// <returns>Returns the throttling scope.</returns>
        public static IThrottlingScope Include(this IThrottlingScope scope, Regex pathRegex, ScopeItemCondition condition)
        {
            if (pathRegex == null)
                throw new ArgumentNullException("pathRegex");

            scope.AddAndOptimizeOrder(new ThrottlingScopeItem()
            {
                Include = true,
                Path = pathRegex.ToString(),
                PathRegex = pathRegex,
                Condition = condition
            });

            return scope;
        }

        /// <summary>
        /// Excludes HTTP request URL path with specified condition from the throttling controller scope.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="path">Path to exclude.</param>
        /// <returns>Returns the throttling scope.</returns>
        public static IThrottlingScope Exclude(this IThrottlingScope scope, string path)
        {
            return scope.Exclude(path, ScopeItemCondition.None);
        }

        /// <summary>
        /// Excludes HTTP request URL path with specified condition from the throttling controller scope.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="path">Path to exclude.</param>
        /// <param name="condition">Condition to apply to the request.</param>
        /// <returns>Returns the throttling scope.</returns>
        public static IThrottlingScope Exclude(this IThrottlingScope scope, string path, ScopeItemCondition condition)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            path = AdjustToVirtualPath(path);

            scope.AddAndOptimizeOrder(new ThrottlingScopeItem()
            {
                Include = false,
                Path = path,
                Condition = condition
            });

            return scope;
        }

        /// <summary>
        /// Excludes HTTP request URL path Regular Expression with specified condition from the throttling controller scope.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="pathRegex">Path Regular Expression to exclude.</param>
        /// <returns>Returns the throttling scope.</returns>
        public static IThrottlingScope Exclude(this IThrottlingScope scope, Regex pathRegex)
        {
            return scope.Exclude(pathRegex, ScopeItemCondition.None);
        }

        /// <summary>
        /// Excludes HTTP request URL path Regular Expression with specified condition from the throttling controller scope.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="pathRegex">Path Regular Expression to exclude.</param>
        /// <param name="condition">Condition to apply to the request.</param>
        /// <returns>Returns the throttling scope.</returns>
        public static IThrottlingScope Exclude(this IThrottlingScope scope, Regex pathRegex, ScopeItemCondition condition)
        {
            if (pathRegex == null)
                throw new ArgumentNullException("pathRegex");

            scope.AddAndOptimizeOrder(new ThrottlingScopeItem()
            {
                Include = false,
                Path = pathRegex.ToString(),
                PathRegex = pathRegex,
                Condition = condition
            });

            return scope;
        }

        /// <summary>
        /// Optimizes the scope items order to allow easier logic at runtime ensuring maximum performance.
        /// </summary>
        internal static void OptimizeScopeItemsOrder(this IThrottlingScope scope)
        {
            List<ThrottlingScopeItem> items = new List<ThrottlingScopeItem>(scope.Items);
            scope.Items.Clear();
            scope.Items.AddRange(items.OrderBy(item => item.Include.ToString()));//exclude - 1 (False); include - 2 (True)
        }

        private static void AddAndOptimizeOrder(this IThrottlingScope scope, ThrottlingScopeItem item)
        {
            scope.Items.Add(item);
            scope.OptimizeScopeItemsOrder();
        }

        private static string AdjustToVirtualPath(string path)
        {
            if (path != "*" && path != "*.*")
            {
                if (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }
            }

            return path;
        }

        internal static ScopeItemCondition ConvertMethodToCondition(this IThrottlingScope scope, string httpMethod)
        {
            ScopeItemCondition methodCondition = ScopeItemCondition.None;
            switch (httpMethod)
            {
                case "GET":
                    methodCondition = ScopeItemCondition.HttpGet;
                    break;
                case "POST":
                    methodCondition = ScopeItemCondition.HttpPost;
                    break;
                case "PUT":
                    methodCondition = ScopeItemCondition.HttpPut;
                    break;
                case "DELETE":
                    methodCondition = ScopeItemCondition.HttpDelete;
                    break;
                case "PATCH":
                    methodCondition = ScopeItemCondition.HttpPatch;
                    break;
                case "TRACE":
                    methodCondition = ScopeItemCondition.HttpTrace;
                    break;
                case "OPTIONS":
                    methodCondition = ScopeItemCondition.HttpOptions;
                    break;
                case "HEAD":
                    methodCondition = ScopeItemCondition.HttpHead;
                    break;
            }
            return methodCondition;
        }
    }
}
