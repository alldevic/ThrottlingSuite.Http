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
using System.Runtime.Serialization;
using System.Xml;
using System.Text.RegularExpressions;

namespace ThrottlingSuite.Core
{
    public class ControllerInstanceInitializer
    {
        internal List<ThrottlingControllerInstance> CreateCollection(ThrottlingConfiguration configuration)
        {
            List<ThrottlingControllerInstance> instances = new List<ThrottlingControllerInstance>();
            XmlElement xmlData = (XmlElement)configuration.InstancesXml;

            if (xmlData == null)
            {
                return instances;
            }

            try
            {
                XmlNodeList nodes = xmlData.SelectNodes("instance");
                ThrottlingControllerInstance instance = null;
                foreach (XmlNode node in nodes)
                {
                    instance = this.CreateInstance((XmlElement)node, configuration.ResourcesCleanupInterval);
                    instance.Controller.ConcurrencyModel = configuration.ConcurrencyModel;
                    instances.Add(instance);
                }

                return instances;
            }
            catch (Exception)
            {
                if (instances != null)
                    instances.ForEach(instance => (instance.Controller as IDisposable).Dispose());

                throw;
            }
        }

        public ThrottlingControllerInstance CreateInstance(XmlElement xmlData, int cleanupInterval)
        {
            IThrottlingController controller = this.CreateController(xmlData, cleanupInterval);
            ThrottlingScope scope = this.CreateScope(xmlData);

            return new ThrottlingControllerInstance(controller, scope);
        }

        public ThrottlingScope CreateScope(XmlElement xmlData)
        {
            ThrottlingScope scope = new ThrottlingScope();
            ThrottlingScopeItem scopeItem = null;
            System.Xml.XmlNode oNode = null;
            foreach (XmlNode scopeItemXml in xmlData.ChildNodes)
            {
                if (scopeItemXml.Name == "clear")
                    scope.Items.Clear();
                else
                {
                    bool include = (scopeItemXml.Name == "add");
                    string path = string.Empty;
                    Regex regex = null;
                    ScopeItemCondition condition = ScopeItemCondition.None;

                    oNode = scopeItemXml.Attributes.GetNamedItem("path");
                    if (oNode != null)
                    {
                        path = oNode.InnerText.Trim();
                    }
                    else
                    {
                        oNode = scopeItemXml.Attributes.GetNamedItem("pathRegex");
                        if (oNode == null)
                            throw new ApplicationException("Cannot find either [add/remove@path], nor [add/remove@pathRegex] attribute in Throttling Instance configuration xml.");
                        else
                        {
                            regex = new Regex(scopeItem.Path, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);
                        }
                    }

                    oNode = scopeItemXml.Attributes.GetNamedItem("condition");
                    if (oNode != null)
                    {
                        oNode.InnerText = oNode.InnerText.Trim();
                        Enum.TryParse<ScopeItemCondition>(oNode.InnerText, true, out condition);
                    }

                    //use Include/Exclude interface to add/remove items
                    if (include)
                    {
                        if (regex == null)
                            scope.Include(path, condition);
                        else
                            scope.Include(regex, condition);
                    }
                    else
                    {
                        if (regex == null)
                            scope.Exclude(path, condition);
                        else
                            scope.Exclude(regex, condition);
                    }
                }
            }
            return scope;
        }

        private IThrottlingController CreateController(XmlElement xmlData, int cleanupInterval)
        {
            int timeIntervalMsec = 0;
            int maxThreshold = 0;
            string throttlingMethod = "ShortAverage";

            if (xmlData == null)
                throw new ArgumentNullException("Xml configuration element cannot be null.");

            System.Xml.XmlNode oNode = xmlData.Attributes.GetNamedItem("timeIntervalMsec");
            if (oNode == null)
                throw new ApplicationException("Cannot find an [timeIntervalMsec] attribute in Throttling Instance configuration xml.");
            timeIntervalMsec = int.Parse(oNode.InnerText.Trim());

            oNode = xmlData.Attributes.GetNamedItem("maxThreshold");
            if (oNode == null)
                throw new ApplicationException("Cannot find an [maxThreshold] attribute in Throttling Instance configuration xml.");
            maxThreshold = int.Parse(oNode.InnerText.Trim());

            oNode = xmlData.Attributes.GetNamedItem("method");
            if (oNode != null)
                throttlingMethod = oNode.InnerText.Trim();

            oNode = xmlData.Attributes.GetNamedItem("name");
            if (oNode == null)
                throw new ApplicationException("Cannot find an [name] attribute in Throttling Instance configuration xml.");
            string name = oNode.InnerText.Trim();

            return CreateController(name, throttlingMethod, timeIntervalMsec, maxThreshold, cleanupInterval);
        }

        private IThrottlingController CreateController(string instanceName, string throttlingMethod, int timeIntervalMsec, int maxThreshold, int cleanupInterval)
        {
            IThrottlingController controller = null;
            switch (throttlingMethod.ToLower())
            {
                case "linear":
                    controller = new LinearThrottlingController(timeIntervalMsec, maxThreshold, cleanupInterval);
                    break;
                case "longaverage":
                    controller = new LongAverageThrottlingController(timeIntervalMsec, maxThreshold, cleanupInterval);
                    break;
                default: //ShortAverage
                    controller = new ShortAverageThrottlingController(timeIntervalMsec, maxThreshold, cleanupInterval);
                    break;
            }

            controller.Name = instanceName;
            return controller;
        }

    }
}
