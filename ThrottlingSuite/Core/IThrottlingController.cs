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
using System.Timers;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// Interface that must be implemented by a throttling controller.
    /// </summary>
    public interface IThrottlingController
    {
        /// <summary>
        /// Gets the time interval in msec that controller uses.
        /// </summary>
        double TimeIntervalMsec { get; }
        /// <summary>
        /// Gets the concurrency model that controller uses.
        /// </summary>
        double MaxThreshold { get; }
        /// <summary>
        /// Gets the date and time when controller was created.
        /// </summary>
        DateTime CreatedDatetime { get; }
        /// <summary>
        /// Gets current size of the dictionary internally used by the controller.
        /// </summary>
        int LookupDictionarySize { get; }
        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Gets total number of calls (requests) evaluated by the controller.
        /// </summary>
        ulong TotalCalls { get; }
        /// <summary>
        /// Gets total number of blocked calls (requests) evaluated by the controller.
        /// </summary>
        ulong BlockedCalls { get; }
        /// <summary>
        /// Method evaluates whether the call (HTTP request) is allowed to go through or should be blocked.
        /// </summary>
        /// <param name="requestSignature">Request signature as a string.</param>
        /// <param name="requestTimestamp">Request time stamp.</param>
        /// <returns>Returns boolean value indicating whether the call (HTTP request) is allowed to go through.</returns>
        bool IsCallAllowed(string requestSignature, DateTime requestTimestamp);
        /// <summary>
        /// Gets the threshold as a number of allowed requests per time interval.
        /// </summary>
        ConcurrencyModel ConcurrencyModel { get; set; }
    }
}
