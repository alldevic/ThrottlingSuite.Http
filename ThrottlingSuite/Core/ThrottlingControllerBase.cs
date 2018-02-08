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
    /// An abstract class containing essential statistical information for the throttling controller.
    /// </summary>
    public abstract class ThrottlingControllerStatus
    {
        /// <summary>
        /// Gets the date and time when controller was created.
        /// </summary>
        public DateTime CreatedDatetime { get; protected set; }
        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets total number of calls (requests) evaluated by the controller. This number is provided for informational purpose only and is not being used internally by throttling controller logic. 
        /// The lock is not used while incrementing this value and therefore it might be slightly inaccurate in high concurrency environment, but generally it does reflect the load on the system.
        /// </summary>
        public ulong TotalCalls { get; protected set; }
        /// <summary>
        /// Gets total number of blocked calls (requests) evaluated by the controller. This number is provided for informational purpose only and is not being used internally by throttling controller logic. 
        /// The lock is not used while incrementing this value and therefore it might be slightly inaccurate in high concurrency environment, but generally it does reflect the load on the system.
        /// </summary>
        public ulong BlockedCalls { get; protected set; }

        /// <summary>
        /// Increments the number of total calls. If maximum value of ULONG type has been reached, it re-sets counters to 0.
        /// </summary>
        protected void IncrementTotalCalls()
        {
            if (this.TotalCalls == ulong.MaxValue)
            {
                this.TotalCalls = 0;
                this.BlockedCalls = 0;
            }
            this.TotalCalls++;
        }

        /// <summary>
        /// Increments the number of blocked calls. If maximum value of ULONG type has been reached, it re-sets counters to 0.
        /// </summary>
        protected void IncrementBlockedCalls()
        {
            if (this.BlockedCalls == ulong.MaxValue)
            {
                this.TotalCalls = 0;
                this.BlockedCalls = 0;
            }
            this.BlockedCalls++;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ThrottlingControllerStatus()
        {
            this.Name = Guid.NewGuid().ToString("N");
            this.CreatedDatetime = DateTime.Now;
        }
    }

    /// <summary>
    /// An abstract class providing a base for the throttling controller.
    /// </summary>
    public abstract class ThrottlingControllerBase : ThrottlingControllerStatus, IThrottlingController, IDisposable
    {
        /// <summary>
        /// Gets the time interval in msec that controller uses.
        /// </summary>
        public double TimeIntervalMsec { get; protected set; }
        /// <summary>
        /// Gets the concurrency model that controller uses.
        /// </summary>
        public ConcurrencyModel ConcurrencyModel { get; set; }
        /// <summary>
        /// Gets the threshold as a number of allowed requests per time interval.
        /// </summary>
        public double MaxThreshold { get; protected set; }

        /// <summary>
        /// The timer used to clean-up internal resources used by an instance of this class.
        /// </summary>
        protected Timer cleanupTimer;

        /// <summary>
        /// Constructor with time interval in msec and threshold as a number of allowed requests per time interval. 
        /// You may supply the cleanup interval in msec - it is used for periodically cleaning up controller resources.
        /// </summary>
        /// <param name="timeIntervalMsec">The time interval in msec.</param>
        /// <param name="maxThreshold">The threshold as a number of allowed requests per time interval.</param>
        /// <param name="cleanupInterval">The cleanup interval in msec. Default value is 30000. Recommended value is between 180000 and 600000 (3 to 10 minutes).</param>
        /// <remarks>To set an unlimited throttling, the timeIntervalMsec and maxThreshold values should be set to -1. 
        /// To prevent specific section of the application from being served, both values should be set to 0.</remarks>
        public ThrottlingControllerBase(int timeIntervalMsec, int maxThreshold, int cleanupInterval)
            : base()
        {
            //validate parameters
            if (timeIntervalMsec != 0 || maxThreshold != 0) //if NOT both values = 0 - fully blocked
            {
                if (timeIntervalMsec != -1 || maxThreshold != -1) //if NOT both values = -1 - unlimited
                {
                    if (timeIntervalMsec <= 0)
                        throw new ArgumentOutOfRangeException("timeIntervalMsec", "Time interval must be greater then 0, or both values, timeIntervalMsec and maxThreshold, should be -1 or 0 for special cases.");

                    if (maxThreshold <= 0)
                        throw new ArgumentOutOfRangeException("maxThreshold", "Maximum threshold must be greater then 0, or both values, timeIntervalMsec and maxThreshold, should be -1 or 0 for special cases.");
                }
            }

            if (cleanupInterval == 0)
                cleanupInterval = ThrottlingConfiguration.DefaultCleanupIntervalMsec;

            this.TimeIntervalMsec = Convert.ToDouble(timeIntervalMsec);
            this.MaxThreshold = Convert.ToDouble(maxThreshold);
            this.ConcurrencyModel = ConcurrencyModel.Optimistic;

            this.cleanupTimer = new Timer();
            this.cleanupTimer.Interval = Convert.ToDouble(cleanupInterval); //5 mins default
            this.cleanupTimer.AutoReset = true;
            this.cleanupTimer.Elapsed += new ElapsedEventHandler(cleanupTimer_Elapsed);
            this.cleanupTimer.Start();
        }

        /// <summary>
        /// Gets current size of the dictionary internally used by the controller.
        /// </summary>
        public virtual int LookupDictionarySize { get { return 0; } }

        /// <summary>
        /// Method evaluates whether the call (HTTP request) is allowed to go through or should be blocked.
        /// </summary>
        /// <param name="requestSignature">Request signature as a string.</param>
        /// <param name="requestTimestamp">Request time stamp.</param>
        /// <returns>Returns boolean value indicating whether the call (HTTP request) is allowed to go through.</returns>
        public abstract bool IsCallAllowed(string requestSignature, DateTime requestTimestamp);

        /// <summary>
        /// An implementation of the clean-up functionality.
        /// </summary>
        protected abstract void ExecuteCleanup();

        /// <summary>
        /// The clean-up timer event handler.
        /// </summary>
        /// <param name="sender">The object raised an event.</param>
        /// <param name="e">The event arguments.</param>
        protected void cleanupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
#if TRACE
                this.WriteTraceMessage(string.Format("Throttling controller [{0}] lookup dictionary cleanup is being called.", this.Name));
#endif
                //stop the timer...
                this.cleanupTimer.Enabled = false;
                ExecuteCleanup();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //start the timer...
                this.cleanupTimer.Enabled = true;
            }
        }

        /// <summary>
        /// Disposes resources used by the throttling controller.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.cleanupTimer != null)
                {
                    this.cleanupTimer.Enabled = false;
                    this.cleanupTimer.Dispose();
                }
            }
        }

        /// <summary>
        /// Writes controller decision-making trace message into a configured trace listener, if available.
        /// </summary>
        /// <remarks>This method is only intended to be used for development/testing purposes.</remarks>
        protected void WriteTraceMessage(string message)
        {
#if TRACE
            System.Diagnostics.Trace.TraceInformation(message);
#endif
        }
    }
}
