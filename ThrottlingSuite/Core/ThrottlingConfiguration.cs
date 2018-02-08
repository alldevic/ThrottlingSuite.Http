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
using System.Xml;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// This class contains Throttling Configuration parameters. 
    /// </summary>
    [Serializable()]
    public class ThrottlingConfiguration
    {
        public static string RequestTimestampPropertyName = "TS_RequestTimestamp";
        public static string CookieTrackingName = "tcm";
        /// <summary>
        /// Gets the default value for cleanup process interval that is used to 
        /// dispose unused data accumulated by the throttling controllers and become outdated.
        /// </summary>
        public static int DefaultCleanupIntervalMsec = 300000; // 5 mins
        /// <summary>
        /// Gets boolean indicator whether throttling is enabled.
        /// </summary>
        /// <remarks>You may enable or disable throttling service during the runtime.</remarks>
        public bool Enabled { get; set; }
        /// <summary>
        /// Gets boolean indicator whether throttling is enabled for logging only.
        /// </summary>
        /// <remarks>You may change this parameter during the runtime.</remarks>
        public bool LogOnly { get; set; }
        /// <summary>
        /// Gets value (in ms) for resources cleanup interval.
        /// </summary>
        /// <remarks>It is not possible to change the resource cleanup interval during the runtime.</remarks>
        public int ResourcesCleanupInterval { get; set; }
        /// <summary>
        /// Gets a type responsible for building HTTP request signature.
        /// </summary>
        /// <remarks>You may not change this parameter during the runtime.</remarks>
        public Type RequestSignatureBuilderType { get; set; }
        /// <summary>
        /// Gets the dictionary contaning parameters for building HTTP request signature.
        /// </summary>
        public SignatureBuilderParameters SignatureBuilderParams { get; private set; }
        /// <summary>
        /// Gets value of concurrency model to use while evaluating data in the throttling controller.
        /// </summary>
        /// <remarks>You may not change this parameter during the runtime.</remarks>
        public ConcurrencyModel ConcurrencyModel { get; set; }

        internal XmlNode InstancesXml { get; set; }
        internal XmlNode AssertConnectionXml { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ThrottlingConfiguration()
        {
            ConcurrencyModel = ConcurrencyModel.Pessimistic;
            RequestSignatureBuilderType = typeof(DefaultRequestSignatureBuilder);
            SignatureBuilderParams = new SignatureBuilderParameters();
            ResourcesCleanupInterval = DefaultCleanupIntervalMsec;
            //set default request signature building params
            RequestSignatureBuilderType = typeof(DefaultRequestSignatureBuilder);
            SignatureBuilderParams.IgnoreAllQueryStringParameters = true;
            SignatureBuilderParams.IgnoreClientIpAddress = true;
            SignatureBuilderParams.EnableClientTracking = false;
            SignatureBuilderParams.UseInstanceUrl = true;
        }

        /// <summary>
        /// Constructor that accepts XML config data and parses it into configuration parameters.
        /// </summary>
        /// <param name="xmlData">XML data to parse.</param>
        public ThrottlingConfiguration(XmlElement xmlData)
            : this()
        {
            if (xmlData == null)
                throw new ArgumentNullException("Xml configuration element cannot be null.");

            XmlNode oNode = xmlData.Attributes.GetNamedItem("enabled");
            if (oNode == null)
                throw new ApplicationException("Cannot find an [enabled] attribute in Throttling xml.");
            Enabled = bool.Parse(oNode.InnerText.Trim());

            oNode = xmlData.Attributes.GetNamedItem("logOnly");
            if (oNode != null)
                LogOnly = bool.Parse(oNode.InnerText.Trim());

            bool enableClientTracking = false;
            oNode = xmlData.Attributes.GetNamedItem("enableClientTracking");
            if (oNode != null)
                enableClientTracking = bool.Parse(oNode.InnerText.Trim());
            SignatureBuilderParams.EnableClientTracking = enableClientTracking;

            oNode = xmlData.Attributes.GetNamedItem("resourcesCleanupInterval");
            if (oNode != null)
                ResourcesCleanupInterval = int.Parse(oNode.InnerText.Trim());
            if (ResourcesCleanupInterval < 60000)
                ResourcesCleanupInterval = 60000; //value cannot be less than 1 min
            else if (ResourcesCleanupInterval > 600000)
                ResourcesCleanupInterval = 600000; //value cannot be greater than 10 min

            oNode = xmlData.Attributes.GetNamedItem("concurrency");
            if (oNode != null)
                ConcurrencyModel = (ConcurrencyModel)Enum.Parse(typeof(ConcurrencyModel), oNode.InnerText.Trim(), true);

            XmlNodeList paramsList = xmlData.SelectNodes("ignoreParameters/add");
            foreach (XmlNode node in paramsList)
            {
                oNode = node.Attributes.GetNamedItem("name");
                if (oNode == null)
                    throw new ApplicationException("Cannot find an [name] attribute in Throttling/ignoreParameters/add node xml.");
                SignatureBuilderParams.IgnoreParameters.Add(oNode.InnerText.Trim());
            }

            oNode = xmlData.SelectSingleNode("instances");
            if (oNode == null)
                throw new ApplicationException("Cannot find an <instances> node in Throttling xml.");
            InstancesXml = oNode;

            oNode = xmlData.SelectSingleNode("assertConnection");
            AssertConnectionXml = oNode;

            oNode = xmlData.SelectSingleNode("requestSignatureBuilder");
            if (oNode != null)
            {
                foreach(XmlAttribute attr in oNode.Attributes)
                {
                    switch (attr.Name)
                    {
                        case "type":
                            RequestSignatureBuilderType = Type.GetType(attr.Value, false, true);
                            break;
                        case "ignoreAllQueryStringParameters":
                            SignatureBuilderParams.IgnoreAllQueryStringParameters = bool.Parse(attr.Value);
                            break;
                        case "ignoreClientIp":
                            SignatureBuilderParams.IgnoreClientIpAddress = bool.Parse(attr.Value);
                            break;
                        case "enableClientTracking":
                            SignatureBuilderParams.EnableClientTracking = bool.Parse(attr.Value);
                            break;
                        case "useInstanceUrl":
                            SignatureBuilderParams.UseInstanceUrl = bool.Parse(attr.Value);
                            break;
                        default:
                            SignatureBuilderParams.Add(attr.Name, attr.Value);
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Concurrency model for the throttling controller.
    /// </summary>
    public enum ConcurrencyModel
    {
        /// <summary>
        /// Use this model when throttling accuracy is not as important as minimum locking of resources. 
        /// </summary>
        /// <remarks>Using this model might cause incorrect throttling, especially in highly concurrent scenarious. 
        /// During tests involved 1,000 calls per second benchmark, the 2.6% error in throttling accuracy has been determined.</remarks>
        Optimistic,
        /// <summary>
        /// [Recommended] Use this model when throttling accuracy is the most important factor.
        /// </summary>
        /// <remarks>This option forces to lock a small section of the code to ensure the throttling accuracy. 
        /// During tests this section has been taken between 30 and 80 micro-seconds per HTTP request that is insignificant for most cases, 
        /// but ensures the accuracy of throttling controllers.</remarks>
        Pessimistic
    }
}
