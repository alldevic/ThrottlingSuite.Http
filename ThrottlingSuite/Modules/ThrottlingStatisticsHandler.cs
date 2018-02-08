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
using System.IO;
using System.Web;
using ThrottlingSuite.Core;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace ThrottlingSuite.Modules
{
    public class ThrottlingStatisticsHandler : IHttpHandler
    {
        public ThrottlingStatisticsHandler()
        {
        }

        public void ProcessRequest(HttpContext context)
        {
            ThrottlingStatisticsAdapter adapter = new ThrottlingStatisticsAdapter();
            ThrottlingStatisticsData data = new ThrottlingStatisticsData();
            if (HttpContext.Current != null)
            {
                object tmp = HttpContext.Current.Application[ThrottlingSuite.Modules.DuplicatedCallsFilter.ThrottlingControllerSuiteAppLocation];
                if (tmp != null)
                {
                    //collect data
                    ThrottlingControllerSuite suite = (ThrottlingControllerSuite)tmp;
                    object filter = HttpContext.Current.Application[ThrottlingSuite.Modules.ClientConnectionAssertionFilter.ClientConnectionAssertionFilterAppLocation];
                    ThrottlingSuite.Modules.ClientConnectionAssertionFilter connectionFilter = null;
                    if (filter != null)
                        connectionFilter = (filter as ThrottlingSuite.Modules.ClientConnectionAssertionFilter);

                    data = adapter.CollectStatistics(suite, new[] { connectionFilter as ThrottlingControllerStatus });
                }
                else
                    data.Message = "The Throttling Controller Suite is not yet created.";
            }
            else
                data.Message = "No HTTP Context to collect Throttling Statistics data from.";

            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(data.GetType());

            if (!string.IsNullOrWhiteSpace(context.Request.QueryString["callback"]))
            {   //handle JSONP
                context.Response.ContentType = "application/javascript";
                context.Response.Write(string.Format("{0}(", context.Request.QueryString["callback"]));
                serializer.WriteObject(context.Response.OutputStream, data);
                context.Response.Write(");");
            }
            else
            {   //handle JSON
                context.Response.ContentType = "application/json";
                serializer.WriteObject(context.Response.OutputStream, data);
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
