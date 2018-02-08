using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ThrottlingSuite.Core;

namespace ThrottlingWebApiTest
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //add throttling handler into the main pipeline (old way)
            //IEnumerable<IThrottlingControllerInstance> instances = new List<IThrottlingControllerInstance>(new[]
            //    {
            //        new ThrottlingControllerInstance(new ShortAverageThrottlingController(1000, 100) { Name = "controller-API1" }, 
            //            new ThrottlingScope()
            //                .Include("api/content")
            //                .Exclude("api/content/logos")),

            //        new ThrottlingControllerInstance(new ShortAverageThrottlingController(1000, 50) { Name = "controller-API2" }, 
            //            new ThrottlingScope()
            //                .Include("api/users", ScopeItemCondition.HttpGet | ScopeItemCondition.HttpOptions)
            //                .Include("api/orders", ScopeItemCondition.HttpGet | ScopeItemCondition.HttpOptions)),

            //        new ThrottlingControllerInstance(new ShortAverageThrottlingController(1000, 10) { Name = "controller-API3" }, 
            //            new ThrottlingScope()
            //                .Include("api/users", ScopeItemCondition.HttpPost | ScopeItemCondition.HttpDelete | ScopeItemCondition.HttpPut)
            //                .Include("api/orders", ScopeItemCondition.HttpPost | ScopeItemCondition.HttpDelete | ScopeItemCondition.HttpPut)),

            //        new ThrottlingControllerInstance(new ShortAverageThrottlingController(1000, 2) { Name = "controller-API" }, 
            //            new ThrottlingScope()
            //                .Include("api"))

            //    });

            //add throttling handler into the main pipeline (new way using short-cuts)
            IEnumerable<IThrottlingControllerInstance> instances = new List<IThrottlingControllerInstance>(new[]
                {
                    ThrottlingControllerInstance.Create<ShortAverageThrottlingController>("controller-API1", 1000, 100)
                            .IncludeInScope("api/content")
                            .ExcludeFromScope("api/content/logos"),

                    ThrottlingControllerInstance.Create<ShortAverageThrottlingController>("controller-API2", 1000, 50)
                            .IncludeInScope("api/users", ScopeItemCondition.HttpGet | ScopeItemCondition.HttpOptions)
                            .IncludeInScope("api/orders", ScopeItemCondition.HttpGet | ScopeItemCondition.HttpOptions),

                    ThrottlingControllerInstance.Create<ShortAverageThrottlingController>("controller-API3", 1000, 10) 
                            .IncludeInScope("api/users", ScopeItemCondition.HttpPost | ScopeItemCondition.HttpDelete | ScopeItemCondition.HttpPut)
                            .IncludeInScope("api/orders", ScopeItemCondition.HttpPost | ScopeItemCondition.HttpDelete | ScopeItemCondition.HttpPut),

                    ThrottlingControllerInstance.Create<ShortAverageThrottlingController>("controller-API", 1000, 2)
                            .IncludeInScope("api")
                });

            ThrottlingConfiguration throttleConfig = new ThrottlingConfiguration()
            {
                ConcurrencyModel = ConcurrencyModel.Pessimistic,
                Enabled = true,
                LogOnly = false
            };
            throttleConfig.SignatureBuilderParams.IgnoreAllQueryStringParameters = true;
            throttleConfig.SignatureBuilderParams.IgnoreClientIpAddress = true;
            throttleConfig.SignatureBuilderParams.EnableClientTracking = false;
            throttleConfig.SignatureBuilderParams.UseInstanceUrl = true;

            //create throttling service
            IThrottlingService throttle = new ThrottlingControllerSuite(throttleConfig, instances);

            ////create throttling service
            //IThrottlingService throttle = new ThrottlingControllerSuite(config.VirtualPathRoot);

            //add throttling handler into the main pipeline
            config.MessageHandlers.Add(new ThrottlingSuite.Http.Handlers.ThrottlingHandler(throttle));

            //set throttling statistics handler for a desired endpoint
            config.Routes.MapHttpRoute(
                name: "ThrottlingStatEndpoint",
                routeTemplate: "internal/throttling",
                defaults: null,
                constraints: null,
                handler: new ThrottlingSuite.Http.Handlers.ThrottlingStatisticsHandler(throttle)
            );
        }
    }
}
