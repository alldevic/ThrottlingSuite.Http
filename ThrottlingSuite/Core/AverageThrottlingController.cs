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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// Class contains an average history data calcualted for a throttling controller. 
    /// </summary>
    /// <remarks>This class is used by Long/Short-AverageThrottlingController.</remarks>
    [Serializable()]
    [DataContract()]
    public class AverageHistory
    {
        /// <summary>
        /// Gets or sets the last data and time the request has been received.
        /// </summary>
        [DataMember(Name = "lastTime", Order = 1)]
        public DateTime LastTime { get; set; }
        /// <summary>
        /// Gets or sets an average timespan between the requests.
        /// </summary>
        [DataMember(Name = "avgTime", Order = 2)]
        public double AverageTime { get; set; }
        /// <summary>
        /// Gets or sets the total number of requests.
        /// </summary>
        [DataMember(Name = "count", Order = 3)]
        public double CurrentCount { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AverageHistory()
        {
            this.LastTime = DateTime.Now;
        }
    }

    /// <summary>
    /// Class implements IThrottlingController using short-average throttlling  algorithm.
    /// </summary>
    /// <remarks>See Throttling Suite documentation for more information: https://throttlewebapi.codeplex.com/wikipage?title=Throttling%20algorithm%20considerations&amp;referringTitle=Documentation. </remarks>
    public class ShortAverageThrottlingController : LongAverageThrottlingController
    {
        /// <summary>
        /// Constructor with time interval in msec and threshold as a number of allowed requests per time interval.
        /// </summary>
        /// <param name="timeIntervalMsec">The time interval in msec.</param>
        /// <param name="maxThreshold">The threshold as a number of allowed requests per time interval.</param>
        public ShortAverageThrottlingController(int timeIntervalMsec, int maxThreshold)
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
        public ShortAverageThrottlingController(int timeIntervalMsec, int maxThreshold, int cleanupInterval)
            : base(timeIntervalMsec, maxThreshold, cleanupInterval)
        {
            this.ApplyLongtimePenalty = false;
        }
    }

    /// <summary>
    /// Class implements IThrottlingController using long-average throttlling  algorithm.
    /// </summary>
    /// <remarks>See Throttling Suite documentation for more information: https://throttlewebapi.codeplex.com/wikipage?title=Throttling%20algorithm%20considerations&amp;referringTitle=Documentation. </remarks>
    public class LongAverageThrottlingController : ThrottlingControllerBase, IDisposable
    {
        private object _locking = new object();

        private double MinAverageThreshold { get; set; }
        /// <summary>
        /// Gets or sets falg indicated that penalty should be applied to calls if user made too many calls at once. 
        /// If false, then history will be reset as soon as Threshold Interval has passed since last call been made. 
        /// If true, then calls will not be allowed for as long as threshold reaches permitted.
        /// </summary>
        protected bool ApplyLongtimePenalty { get; set; }

        private ConcurrentDictionary<string, AverageHistory> RequestsStat { get; set; }
        /// <summary>
        /// Gets current size of the dictionary internally used by the controller.
        /// </summary>
        public override int LookupDictionarySize { get { return this.RequestsStat.Count; } }

        /// <summary>
        /// Constructor with time interval in msec and threshold as a number of allowed requests per time interval. 
        /// </summary>
        /// <param name="timeIntervalMsec">The time interval in msec.</param>
        /// <param name="maxThreshold">The threshold as a number of allowed requests per time interval.</param>
        public LongAverageThrottlingController(int timeIntervalMsec, int maxThreshold)
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
        public LongAverageThrottlingController(int timeIntervalMsec, int maxThreshold, int cleanupInterval)
            : base(timeIntervalMsec, maxThreshold, cleanupInterval)
        {
            this.RequestsStat = new ConcurrentDictionary<string, AverageHistory>();
            this.MinAverageThreshold = this.TimeIntervalMsec / this.MaxThreshold;
            this.ApplyLongtimePenalty = true;
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
            if (this.TimeIntervalMsec == 0 || this.MaxThreshold == 0)
                return false;
            if (this.TimeIntervalMsec == -1 || this.MaxThreshold == -1)
                return true;
            //follow up with rules
            bool isAllowed = true;
            AverageHistory historyCopy = new AverageHistory()
                {
                    LastTime = requestTimestamp,
                    CurrentCount = 1,
                    AverageTime = this.TimeIntervalMsec
                };

            double elapsedTime = this.TimeIntervalMsec;

#region Critical section begins
            if (this.ConcurrencyModel == Core.ConcurrencyModel.Optimistic)
            {
                CalculateAndUpdateStat(requestSignature, requestTimestamp, ref isAllowed, historyCopy, ref elapsedTime);
            }
            else
            {
                lock (_locking)
                {
                    CalculateAndUpdateStat(requestSignature, requestTimestamp, ref isAllowed, historyCopy, ref elapsedTime);
                }
            }
#endregion Critical section ends
#if TRACE
            base.WriteTraceMessage(string.Format("Request: {0}; elapsedTime: {1}; {2} by [{3}]; (AverageTime: {4})", requestSignature, elapsedTime.ToString(), isAllowed ? "ALLOWED" : "BLOCKED", this.Name, historyCopy.AverageTime.ToString()));
#endif
            this.IncrementTotalCalls();
            if (!isAllowed)
                this.IncrementBlockedCalls();
            return isAllowed;
        }

        private void CalculateAndUpdateStat(string requestSignature, DateTime requestTimestamp, ref bool isAllowed, AverageHistory historyCopy, ref double elapsedTime)
        {
            AverageHistory history = null;
            if (this.RequestsStat.TryGetValue(requestSignature, out history))
            {
                historyCopy.LastTime = requestTimestamp;
                historyCopy.AverageTime = history.AverageTime;
                historyCopy.CurrentCount = history.CurrentCount;
                elapsedTime = requestTimestamp.Subtract(history.LastTime).TotalMilliseconds;
                if (elapsedTime < 0)
                    elapsedTime = 0; //work around for 2 parallel exactly sync calls (rare case, but should be handled)

                double newAverageTime = ((historyCopy.AverageTime * historyCopy.CurrentCount) + elapsedTime) / (historyCopy.CurrentCount + 1.0);
                historyCopy.AverageTime = newAverageTime;
                historyCopy.CurrentCount++;
                isAllowed = (newAverageTime >= this.MinAverageThreshold);
                if (historyCopy.CurrentCount == this.MaxThreshold)
                {   //we use TimeIntervalMsec as default value for 1st HTTP request; now we have full sequence, recalculate AVG time using Default (MIN) AVG
                    newAverageTime = ((historyCopy.AverageTime * historyCopy.CurrentCount) - (this.TimeIntervalMsec - this.MinAverageThreshold)) / historyCopy.CurrentCount;
                    historyCopy.AverageTime = newAverageTime;
                }
                //if time since last call has been longer than threshold interval and ApplyPenalty is not set, then reset history
                if (elapsedTime > this.TimeIntervalMsec && !this.ApplyLongtimePenalty)
                {
                    historyCopy.AverageTime = this.TimeIntervalMsec;
                    historyCopy.CurrentCount = 1.0;
                    isAllowed = true;
                }
            }

            //record new history if call is allowed OR Longtime Penalty should be applied
            if (isAllowed || this.ApplyLongtimePenalty)
            {
                double elapsedTime2 = elapsedTime;
                this.RequestsStat.AddOrUpdate(requestSignature, historyCopy, (key, existingHistory) =>
                {
                    if (existingHistory == null)
                        return historyCopy;
                    bool isExistingNewer = (history == null) ? false : (existingHistory.LastTime > history.LastTime);
                    if (isExistingNewer || existingHistory.LastTime > requestTimestamp)
                    {   //if newer history is already recorded, then recalculate
                        double recalculatedAverageTime = ((existingHistory.AverageTime * existingHistory.CurrentCount) + elapsedTime2) / (existingHistory.CurrentCount + 1.0);
                        existingHistory.AverageTime = recalculatedAverageTime;
                        existingHistory.CurrentCount++;
                        return existingHistory;
                    }
                    else
                        return historyCopy;
                });
            }
        }

        protected override void ExecuteCleanup()
        {
            if (this.RequestsStat == null)
                return;

            string key = "";
            DateTime now = DateTime.Now;
            AverageHistory tmp = null;
            double lifetime = this.ApplyLongtimePenalty ? 300 * 1000 : this.TimeIntervalMsec * 2; //either 5 mins or 2x time interval
            int totalRemoved = 0;
            for (int ii = 0; ii < this.RequestsStat.Count; ii++)
            {
                key = this.RequestsStat.Keys.ElementAt(ii);
                if (now.Subtract(this.RequestsStat[key].LastTime).TotalMilliseconds > lifetime)
                {
                    if (this.RequestsStat.TryRemove(key, out tmp))
                    {
                        ii--;
                        totalRemoved++;
                    }
                }
            }
#if TRACE
            base.WriteTraceMessage(string.Format("Throttling controller [{1}] lookup dictionary cleanup is complete. {0} items have beed removed.", totalRemoved.ToString(), this.Name));
#endif
        }

        /// <summary>
        /// Disposes resources used by the throttling controller.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            this.RequestsStat.Clear();
            base.Dispose(disposing);
        }
    }
}
