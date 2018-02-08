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
using System.Web;
using System.Configuration;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Web.Configuration;
using ThrottlingSuite.Core;

namespace ThrottlingSuite.Modules
{
    public class DuplicatedCallsFilter : IHttpModule
    {
        public static string ThrottlingControllerSuiteAppLocation = "App_ThrottlingControllerSuite";

        private readonly static object _syncObject = new object();

        private ThrottlingControllerSuite Controller;
        private IRequestSignatureBuilder RequestSignatureBuilder;
        private string SessionCookieName;

        public void Init(HttpApplication context)
        {
            //create throttling controller, but assure single instance per application domain.
            if (this.Controller == null)
            {
#if TRACE
                Trace.CorrelationManager.StartLogicalOperation(ThrottlingControllerSuiteAppLocation + " initialization.");
#endif
                lock (_syncObject)
                {
                    ThrottlingConfiguration configuration = ThrottlingConfigurationProvider.GetCurrentConfiguration();
                    if (context.Application[ThrottlingControllerSuiteAppLocation] == null)
                    {
                        context.Application.Lock();
                        context.Application.Add(ThrottlingControllerSuiteAppLocation, new ThrottlingControllerSuite(configuration));
                        context.Application.UnLock();
                    }
                    this.Controller = (ThrottlingControllerSuite)context.Application[ThrottlingControllerSuiteAppLocation];
                    this.RequestSignatureBuilder = (IRequestSignatureBuilder)Activator.CreateInstance(configuration.RequestSignatureBuilderType);
                    this.RequestSignatureBuilder.Init(configuration);
#if TRACE
                    Trace.TraceInformation(ThrottlingControllerSuiteAppLocation + " is created with DuplicatedCallsFilter.");
#endif
                }
            }

            context.BeginRequest += new EventHandler(context_BeginRequest);
            SessionStateSection SessionSettings = (SessionStateSection)ConfigurationManager.GetSection("system.web/sessionState");
            this.SessionCookieName = SessionSettings.CookieName;
#if TRACE
            Trace.TraceInformation("DuplicatedCallsFilter is initialized.");
            Trace.CorrelationManager.StopLogicalOperation();
#endif
        }

        private void context_BeginRequest(object source, EventArgs e)
        {
#if TRACE
            Stopwatch watch = null;
#endif
            string requestSignature = "";
            try
            {
#if TRACE
                Trace.CorrelationManager.StartLogicalOperation("Throttling Suite processing.");
#endif
                if (!this.Controller.Configuration.Enabled)
                {
#if TRACE
                    Trace.TraceInformation("Throttling functionality is currently disabled.");
                    Trace.CorrelationManager.StopLogicalOperation();
#endif
                    return;
                }
#if TRACE
                watch = new Stopwatch();
                watch.Start();
#endif
                HttpApplication application = (HttpApplication)source;
                HttpContext context = application.Context;

                //compute request signature
                requestSignature = this.RequestSignatureBuilder.ComputeRequestSignature(context);

                //if no ASP.NET_SessionId cookies exists, add TCM (throttling control marker) value to mark all requests from specific client.
                bool hasSessionId = (context.Request.Cookies[SessionCookieName] != null || context.Request.Cookies[ThrottlingConfiguration.CookieTrackingName] != null);
                if (!hasSessionId)
                    context.Response.Cookies.Add(new HttpCookie(ThrottlingConfiguration.CookieTrackingName, Guid.NewGuid().ToString()));

                //verify if call can go through
                string blockedInstanceName = "";
                if (!this.Controller.IsCallAllowed(context, requestSignature, hasSessionId, out blockedInstanceName))
                {
                    context.Response.AppendToLog(string.Format("[BLOCKED:{0}]", blockedInstanceName));
                    if (!this.Controller.Configuration.LogOnly)
                    {
                        context.Response.StatusCode = 429;
                        context.Response.StatusDescription = "Too Many Requests";
                        application.CompleteRequest();
                    }
                }
            }
            finally
            {
#if TRACE
                if (watch != null)
                {
                    watch.Stop();
                    Trace.TraceInformation("Request: {0}; DuplicatedCallsFilter complete in {1} ms.", requestSignature, watch.Elapsed.TotalMilliseconds.ToString());
                }
                Trace.CorrelationManager.StopLogicalOperation();
#endif
            }
        }

        public void Dispose()
        {
            if (this.Controller != null)
                (this.Controller as IDisposable).Dispose();
        }
    }
}
