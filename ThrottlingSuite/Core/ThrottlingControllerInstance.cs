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
using System.Xml;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// This interface should be implemented by a class providing a throttling controller unit as throttling controller paired with scope evaluation class. 
    /// The controller instance is responsible for throttling a HTTP request if request is within the instance's scope.
    /// </summary>
    public interface IThrottlingControllerInstance
    {
        /// <summary>
        /// Gets the throttling controller that instance uses.
        /// </summary>
        IThrottlingController Controller { get; }
        /// <summary>
        /// Gets the throttling scope evaluation class that instance uses.
        /// </summary>
        IThrottlingScope Scope { get; }
        /// <summary>
        /// Gets the name of the instance.
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    /// This class provides a throttling controller unit as throttling controller paired with scope evaluation class. 
    /// The controller instance is responsible for throttling a HTTP request if request is within the instance's scope.
    /// </summary>
    public class ThrottlingControllerInstance : IThrottlingControllerInstance
    {
        /// <summary>
        /// Gets the throttling controller that instance uses.
        /// </summary>
        public IThrottlingController Controller { get; private set; }
        /// <summary>
        /// Gets the throttling scope evaluation class that instance uses.
        /// </summary>
        public IThrottlingScope Scope { get; private set; }
        /// <summary>
        /// Gets the name of the instance.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor that accepts throttling controller and it's scope evaluation class to pair with.
        /// </summary>
        /// <param name="controller">The throttling controller.</param>
        /// <param name="scope">The throttling scope evaluation class.</param>
        public ThrottlingControllerInstance(IThrottlingController controller, IThrottlingScope scope)
        {
            if (controller == null)
                throw new ArgumentNullException("controller");

            if (scope == null)
                throw new ArgumentNullException("scope");

            if (string.IsNullOrWhiteSpace(controller.Name))
                throw new ArgumentException("Name is required for controller.", "controller");

            this.Controller = controller;
            this.Name = this.Controller.Name;
            this.Scope = scope;
            this.Scope.OptimizeScopeItemsOrder();
        }

        /// <summary>
        /// Creates an instance of throttling controller with time interval in msec and threshold as a number of allowed requests per time interval. 
        /// The throttling scope is empty once an instance created and should be populated with throttling scope items.
        /// </summary>
        /// <param name="name">The name for controller instance.</param>
        /// <param name="timeIntervalMsec">The time interval in msec.</param>
        /// <param name="maxThreshold">The threshold as a number of allowed requests per time interval.</param>
        /// <remarks>To set an unlimited throttling, the timeIntervalMsec and maxThreshold values should be set to -1. 
        /// To prevent specific section of the application from being served, both values should be set to 0.</remarks>
        public static IThrottlingControllerInstance Create<T>(string name, int timeIntervalMsec, int maxThreshold)
            where T : IThrottlingController
        {
            return Create<T>(name, timeIntervalMsec, maxThreshold, ThrottlingConfiguration.DefaultCleanupIntervalMsec);
        }

        /// <summary>
        /// Creates an instance of throttling controller with time interval in msec and threshold as a number of allowed requests per time interval. 
        /// Supply the cleanup interval in msec - it is used for periodically cleaning up controller resources. 
        /// The throttling scope is empty once an instance created and should be populated with throttling scope items.
        /// </summary>
        /// <param name="name">The name for controller instance.</param>
        /// <param name="timeIntervalMsec">The time interval in msec.</param>
        /// <param name="maxThreshold">The threshold as a number of allowed requests per time interval.</param>
        /// <param name="cleanupInterval">The cleanup interval in msec. Default value is 30000. Recommended value is between 180000 and 600000 (3 to 10 minutes).</param>
        /// <remarks>To set an unlimited throttling, the timeIntervalMsec and maxThreshold values should be set to -1. 
        /// To prevent specific section of the application from being served, both values should be set to 0.</remarks>
        public static IThrottlingControllerInstance Create<T>(string name, int timeIntervalMsec, int maxThreshold, int cleanupInterval)
            where T : IThrottlingController
        {
            var type = new Type[] { typeof(int), typeof(int), typeof(int) };
            var pars = new object[] { timeIntervalMsec, maxThreshold, cleanupInterval };
            T controller = (T)(typeof(T)).GetConstructor(type).Invoke(pars);

            controller.Name = name;
            ThrottlingControllerInstance instance = new ThrottlingControllerInstance(controller, new ThrottlingScope());

            return instance;
        }
    }
}
