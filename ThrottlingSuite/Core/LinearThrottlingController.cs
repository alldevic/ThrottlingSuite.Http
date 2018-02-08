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
using System.Collections.Concurrent;
using System.Linq;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// Class implements IThrottlingController using linear throttlling  algorithm.
    /// </summary>
    /// <remarks>See Throttling Suite documentation for more information: https://throttlewebapi.codeplex.com/wikipage?title=Throttling%20algorithm%20considerations&amp;referringTitle=Documentation. </remarks>
    public class LinearThrottlingController : ThrottlingControllerBase
    {
        private object _locking = new object();

        private double LinearThreshold { get; set; }
        private ConcurrentDictionary<string, DateTime> RequestsStat;
        /// <summary>
        /// Gets current size of the dictionary internally used by the controller.
        /// </summary>
        public override int LookupDictionarySize { get { return RequestsStat.Count; } }

        /// <summary>
        /// Constructor with time interval in msec and threshold as a number of allowed requests per time interval. 
        /// </summary>
        /// <param name="timeIntervalMsec">The time interval in msec.</param>
        /// <param name="maxThreshold">The threshold as a number of allowed requests per time interval.</param>
        public LinearThrottlingController(int timeIntervalMsec, int maxThreshold)
            : this(timeIntervalMsec, maxThreshold, ThrottlingConfiguration.DefaultCleanupIntervalMsec)
        {
        }

        /// <summary>
        /// Constructor with time interval in msec and threshold as a number of allowed requests per time interval. 
        /// You may supply the cleanup interval in msec - it is used for periodically cleaning up controller resources.
        /// </summary>
        /// <param name="timeIntervalMsec">The time interval in msec.</param>
        /// <param name="maxThreshold">The threshold as a number of allowed requests per time interval.</param>
        /// <param name="cleanupInterval">The cleanup interval in msec. Recommended value is between 180000 and 600000 (3 to 10 minutes).</param>
        public LinearThrottlingController(int timeIntervalMsec, int maxThreshold, int cleanupInterval)
            : base(timeIntervalMsec, maxThreshold, cleanupInterval)
        {
            RequestsStat = new ConcurrentDictionary<string, DateTime>();
            LinearThreshold = TimeIntervalMsec / MaxThreshold;
        }

        /// <summary>
        /// Method evaluates whether the call (HTTP request) is allowed to go through or should be blocked.
        /// </summary>
        /// <param name="requestSignature">Request signature as a string.</param>
        /// <param name="requestTimestamp">Request time stamp.</param>
        /// <returns>Returns boolean value indicating whether the call (HTTP request) is allowed to go through.</returns>
        public override bool IsCallAllowed(string requestSignature, DateTime requestTimestamp)
        {
            //check defaults
            if (TimeIntervalMsec == 0 || MaxThreshold == 0)
                return false;
            if (TimeIntervalMsec == -1 || MaxThreshold == -1)
                return true;
            //follow up with rules
            bool isAllowed = true;
            DateTime currentDTStamp = requestTimestamp;
            DateTime dtStamp = CreatedDatetime;
            double elapsedTime = LinearThreshold;

#region Critical section begins
            if (ConcurrencyModel == ConcurrencyModel.Optimistic)
            {
                CalculateAndUpdateStat(requestSignature, ref isAllowed, currentDTStamp, ref dtStamp, ref elapsedTime);
            }
            else
            {
                lock (_locking)
                {
                    CalculateAndUpdateStat(requestSignature, ref isAllowed, currentDTStamp, ref dtStamp, ref elapsedTime);
                }
            }
#endregion Critical section ends

#if TRACE
            WriteTraceMessage(string.Format("Request: {0}; elapsedTime: {1}; {2} by [{3}].", requestSignature, elapsedTime.ToString(), isAllowed ? "ALLOWED" : "BLOCKED", Name));
#endif
            IncrementTotalCalls();
            if (!isAllowed)
                IncrementBlockedCalls();
            return isAllowed;
        }

        private void CalculateAndUpdateStat(string requestSignature, ref bool isAllowed, DateTime currentDTStamp, ref DateTime dtStamp, ref double elapsedTime)
        {
            if (RequestsStat.TryGetValue(requestSignature, out dtStamp))
            {
                elapsedTime = currentDTStamp.Subtract(dtStamp).TotalMilliseconds;
                isAllowed = (elapsedTime >= LinearThreshold);
            }
            //record new history if call is allowed
            if (isAllowed)
            {
                RequestsStat.AddOrUpdate(requestSignature, currentDTStamp,
                    (key, existingStamp) =>
                    {//if newer history is already recorded, then keep newer, otherwise use from current HTTP request
                        if (existingStamp == null)
                            return currentDTStamp;
                        return existingStamp > currentDTStamp ? existingStamp : currentDTStamp;
                    });
            }
        }

        /// <summary>
        /// Implements the resource clean-up process.
        /// </summary>
        protected override void ExecuteCleanup()
        {
            if (RequestsStat == null)
                return;

            string key = "";
            DateTime now = DateTime.Now;
            DateTime tmp = DateTime.MinValue;
            double lifetime = TimeIntervalMsec * 2;
            int totalRemoved = 0;
            for (int ii = 0; ii < RequestsStat.Count; ii++)
            {
                key = RequestsStat.Keys.ElementAt(ii);
                if (now.Subtract(RequestsStat[key]).TotalMilliseconds > lifetime)
                {
                    if (RequestsStat.TryRemove(key, out tmp))
                    {
                        ii--; 
                        totalRemoved++;
                    }
                }
            }
#if TRACE
            WriteTraceMessage(string.Format("Throttling controller [{1}] lookup dictionary cleanup is complete. {0} items have beed removed.", totalRemoved.ToString(), Name));
#endif
        }

        /// <summary>
        /// Disposes resources used by the throttling controller.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            RequestsStat.Clear();
            base.Dispose(disposing);
        }
    }
}
