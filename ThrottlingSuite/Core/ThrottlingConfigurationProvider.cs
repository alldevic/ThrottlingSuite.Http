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
using System.Configuration;
using System.Xml;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// Class provides access to XML configuration in the .config file.
    /// </summary>
    public class ThrottlingConfigurationProvider
    {
        /// <summary>
        /// Gets current throttling configuration.
        /// </summary>
        public static ThrottlingConfiguration GetCurrentConfiguration()
        {
            return (ThrottlingConfiguration)ConfigurationManager.GetSection("throttling");
        }
    }

    [Serializable()]
    public class ThrottlingConfigurationHandler : IConfigurationSectionHandler
    {
        public ThrottlingConfigurationHandler()
        {
        }

        public object Create(object parent, object configContext, XmlNode section)
        {
            XmlElement root = (XmlElement)section;
            return new ThrottlingConfiguration(root);
        }
    }
}
