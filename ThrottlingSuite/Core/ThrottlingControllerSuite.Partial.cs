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
    /// Class represents the suite of throttling controllers and design to perform evaluation for all controllers within the suite. 
    /// Class provides implementation for IThrottlingService interface.
    /// </summary>
    public partial class ThrottlingControllerSuite: IDisposable
    {
        /// <summary>
        /// Gets the configuration. Changing configuration parameters at the runtime may not take an effect.
        /// </summary>
        public ThrottlingConfiguration Configuration { get; private set; }
        /// <summary>
        /// Gets the collection of throttling controllers.
        /// </summary>
        public List<IThrottlingControllerInstance> Instances { get; private set; }
        private Dictionary<string, IThrottlingController> InstancesLookup { get; set; }

        /// <summary>
        /// Default constructor. 
        /// This constructor would load configuration from XML config file and create throttling controller instances from this configuration object.
        /// </summary>
        public ThrottlingControllerSuite()
            : this(ThrottlingConfigurationProvider.GetCurrentConfiguration())
        {
        }

        /// <summary>
        /// Constructor accepting the throttling configuration. 
        /// This constructor would create throttling controller instances from the configuration object assuming it has been loaded from XML config file.
        /// </summary>
        /// <param name="configuration">Throttling configuration object.</param>
        public ThrottlingControllerSuite(ThrottlingConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            this.Configuration = configuration;
            //create instances from configuration
            ControllerInstanceInitializer initializer = new ControllerInstanceInitializer();
            List<ThrottlingControllerInstance> collection = initializer.CreateCollection(this.Configuration);
            this.SetInstances(collection);
        }

        /// <summary>
        /// Constructor accepting the throttling configuration and collection of throttling controller instances. 
        /// </summary>
        /// <param name="configuration">Throttling configuration object.</param>
        /// <param name="instances">The collection of throttling controller instances.</param>
        public ThrottlingControllerSuite(ThrottlingConfiguration configuration, IEnumerable<IThrottlingControllerInstance> instances)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            if (instances == null)
                throw new ArgumentNullException("instances");

            this.Configuration = configuration;
            this.SetInstances(instances);
            //ensure configuration in sync
            this.Instances.ForEach(ins => ins.Controller.ConcurrencyModel = configuration.ConcurrencyModel);
        }

        private void SetInstances(IEnumerable<IThrottlingControllerInstance> instances)
        {
            this.Instances = new List<IThrottlingControllerInstance>();
            this.Instances.AddRange(instances);

            this.InstancesLookup = new Dictionary<string, IThrottlingController>();
            this.Instances.ForEach(instance => this.InstancesLookup.Add(instance.Name, instance.Controller));
        }

        /// <summary>
        /// Method returns a throttling controller within the suite by its instance name.
        /// </summary>
        /// <param name="instanceName">The throttling controller instance name.</param>
        /// <returns>Return an instance of IThrottlingController.</returns>
        public IThrottlingController GetControllerByName(string instanceName)
        {
            if (string.IsNullOrWhiteSpace(instanceName))
                throw new ArgumentException("Value should not be null, an empty string, or a white space string.", "instanceName");

            if (this.InstancesLookup.ContainsKey(instanceName))
                return this.InstancesLookup[instanceName];

            return null;
        }

        /// <summary>
        /// Disposes resources used by the object.
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
                if (this.Instances != null)
                    this.Instances.ForEach(instance => (instance.Controller as IDisposable).Dispose());
            }
        }
    }
}
