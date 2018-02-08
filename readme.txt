
////////////////////////////////////////////////////////////////////////////////////////

ThrottlingSuite.Http - Contains implementation that is specific for Web API. 
                       This VS project uses some of classes linked from the ThrottlingSuite 
                       VS project. It produces single .dll as result of compilation.
                       The linked files are used to avoid additional dependency 
                       on System.Web.dll.

ThrottlingSuite - Contains core classes for throttling functionality.
                  (NOTE: Because this code source is also used as part
                  of the "Throttling Suite for ASP.NET Applications" CodePlex project,
                  this VS project contains implementation that is specific 
                  for general ASP.NET HTTP pipeline.)

ThrottlingSuite.Http.Tests - Unit tests for Web API implementation, 
                             including performance validation;

ThrottlingSuiteUnitTests - Unit tests for throttling controllers;

ThrottlingWebApiTest - simple demo Web API application; it is used for testing as well.

////////////////////////////////////////////////////////////////////////////////////////

