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
using System.Web;
using System.Diagnostics;
using ThrottlingSuite.Core;
using System.Xml;

namespace ThrottlingSuite.Modules
{
    public class ClientConnectionAssertionFilter : ThrottlingControllerStatus, IHttpModule
    {
        public static string ClientConnectionAssertionFilterAppLocation = "App_ClientConnectionAssertionFilter";

        private ThrottlingScope scope;
        private ThrottlingConfiguration Configuration;

        public ClientConnectionAssertionFilter()
            : base()
        {
            Name = "Client Connection Assertion Filter";
        }

        public void Init(HttpApplication context)
        {
            Configuration = ThrottlingConfigurationProvider.GetCurrentConfiguration();
            if (Configuration.AssertConnectionXml != null)
            {
                XmlElement xmlData = (XmlElement)Configuration.AssertConnectionXml;
                ControllerInstanceInitializer initializer = new ControllerInstanceInitializer();
                scope = initializer.CreateScope(xmlData);
                context.Application.Lock();
                context.Application.Add(ClientConnectionAssertionFilterAppLocation, this);
                context.Application.UnLock();
#if TRACE
                Trace.WriteLine(ClientConnectionAssertionFilterAppLocation + " is created with ClientConnectionAssertionFilter.");
#endif
                context.BeginRequest += new EventHandler(context_BeginRequest);
                context.PreRequestHandlerExecute += new EventHandler(context_PreRequestHandlerExecute);
            }
        }

        private void context_BeginRequest(object source, EventArgs e)
        {
            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;

            if (scope.InScope(context, false))
            {
                context.Items["ClientConnectionAssertionFilter_InScope"] = true;
                IncrementTotalCalls();
                //verify if call can go through
                if (!context.Response.IsClientConnected)
                {
                    IncrementBlockedCalls();
                    context.Response.AppendToLog("[DISCONNECTED:1]");
                    if (!Configuration.LogOnly)
                        application.CompleteRequest();
                }
            }
        }

        private void context_PreRequestHandlerExecute(object source, EventArgs e)
        {
            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;

            if (context.Items.Contains("ClientConnectionAssertionFilter_InScope"))
            {
                //verify if call can go through
                if (!context.Response.IsClientConnected)
                {
                    IncrementBlockedCalls();
                    context.Response.AppendToLog("[DISCONNECTED:2]");
                    if (!Configuration.LogOnly)
                        application.CompleteRequest();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
