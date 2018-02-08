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

namespace ThrottlingSuite.Core
{
    public class ThrottlingStatisticsAdapter
    {
        public ThrottlingStatisticsAdapter()
        {
        }

        public ThrottlingStatisticsData CollectStatistics(IThrottlingService suite, IEnumerable<ThrottlingControllerStatus> filtersStatus)
        {
            ThrottlingStatisticsData data = new ThrottlingStatisticsData();
            if (suite != null)
            {
                    //collect data
                    data.Enabled = suite.Configuration.Enabled;
                    data.MachineName = Environment.MachineName;
                    data.LogOnly = suite.Configuration.LogOnly;
                    data.IgnoreParametersCount = suite.Configuration.SignatureBuilderParams.IgnoreParameters.Count;
                    data.TotalInstances = suite.Instances.Count;

                    InstancesStatisticsData instanceData = null;
                    foreach (ThrottlingControllerInstance instance in suite.Instances)
                    {
                        instanceData = new InstancesStatisticsData();
                        instanceData.Name = instance.Name;
                        instanceData.CreatedDatetime = instance.Controller.CreatedDatetime;
                        instanceData.ElapsedSeconds = Math.Round(DateTime.Now.Subtract(instanceData.CreatedDatetime).TotalSeconds, 1);
                        instanceData.TimeIntervalMsec = instance.Controller.TimeIntervalMsec;
                        instanceData.MaxThreshold = instance.Controller.MaxThreshold;
                        instanceData.LookupDictionarySize = instance.Controller.LookupDictionarySize;
                        instanceData.TotalCalls = instance.Controller.TotalCalls;
                        instanceData.BlockedCalls = instance.Controller.BlockedCalls;
                        data.Instances.Add(instanceData);
                    }

                    if (filtersStatus != null)
                    {
                        foreach (ThrottlingControllerStatus status in filtersStatus)
                        {
                            instanceData = new InstancesStatisticsData();
                            instanceData.Name = status.Name;
                            instanceData.CreatedDatetime = status.CreatedDatetime;
                            instanceData.ElapsedSeconds = Math.Round(DateTime.Now.Subtract(status.CreatedDatetime).TotalSeconds, 1);
                            instanceData.TotalCalls = status.TotalCalls;
                            instanceData.BlockedCalls = status.BlockedCalls;
                            data.Instances.Add(instanceData);
                        }
                    }

                    data.Message = "Complete.";
            }
            else
            {
                    data.Message = "The Throttling Controller Suite is not set.";
            }
            return data;
        }
    }
}
