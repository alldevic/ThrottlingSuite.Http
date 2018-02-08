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
using System.Runtime.Serialization;

namespace ThrottlingSuite.Core
{
    [Serializable()]
    [DataContract()]
    public class ThrottlingStatisticsData
    {
        [DataMember(Name = "enabled", Order = 1)]
        public bool Enabled { get; set; }
        [DataMember(Name = "logOnly", Order = 2)]
        public bool LogOnly { get; set; }
        [DataMember(Name = "ignoreParamsCount", Order = 3)]
        public int IgnoreParametersCount { get; set; }
        [DataMember(Name = "totalInstances", Order = 4)]
        public int TotalInstances { get; set; }
        [DataMember(Name = "instances", Order = 5)]
        public List<InstancesStatisticsData> Instances { get; set; }
        [DataMember(Name = "message", Order = 6)]
        public string Message { get; set; }
        [DataMember(Name = "machine", Order = 7)]
        public string MachineName { get; set; }

        public ThrottlingStatisticsData()
        {
            Message = "";
            Instances = new List<InstancesStatisticsData>();
        }
    }

    [Serializable()]
    [DataContract()]
    public class InstancesStatisticsData
    {
        [DataMember(Name = "name", Order = 1)]
        public string Name { get; set; }
        [DataMember(Name = "created", Order = 2)]
        public DateTime CreatedDatetime { get; set; }
        [DataMember(Name = "elapsed", Order = 3)]
        public double ElapsedSeconds { get; set; }
        [DataMember(Name = "maxThreshold", Order = 4)]
        public double MaxThreshold { get; set; }
        [DataMember(Name = "timeIntervalMsec", Order = 5)]
        public double TimeIntervalMsec { get; set; }
        [DataMember(Name = "dictionarySize", Order = 6)]
        public int LookupDictionarySize { get; set; }
        [DataMember(Name = "totalCalls", Order = 7)]
        public ulong TotalCalls { get; set; }
        [DataMember(Name = "blockedCalls", Order = 8)]
        public ulong BlockedCalls { get; set; }

        public InstancesStatisticsData()
        {
            Name = "";
            CreatedDatetime = DateTime.MinValue;
        }

    }
}
