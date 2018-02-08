using System.Collections.Generic;
using ThrottlingSuite.Core;

namespace ThrottlingSuite.Http.Tests
{
    public class ThrottlingServiceTestHelper
    {
        public IThrottlingService SetupThrottle()
        {
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

                    ThrottlingControllerInstance.Create<ShortAverageThrottlingController>("controller-API", 1000, 10000)
                            .ExcludeFromScope("apiv2")
                            .IncludeInScope("v3")
                            .IncludeInScope("v5")
                            .IncludeInScope("api"),

                    ThrottlingControllerInstance.Create<ShortAverageThrottlingController>("controller-API4", 1000, 2)
                            .IncludeInScope("testapi")
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

            return throttle;
        }
    }
}
