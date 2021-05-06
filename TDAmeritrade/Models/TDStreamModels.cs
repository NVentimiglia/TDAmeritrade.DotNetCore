using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TDAmeritrade
{
    public class TDRealtimeParams
    {
        public string credential { get; set; }
        public string token { get; set; }
        public string version { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string keys { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string fields { get; set; }
    }

    public class TDRealtimeRequest
    {
        public string service { get; set; }
        public string command { get; set; }
        public int requestid { get; set; }
        public string account { get; set; }
        public string source { get; set; }
        public TDRealtimeParams parameters { get; set; }
    }

    public class TDRealtimeRequestContainer
    {
        public TDRealtimeRequest[] requests { get; set; }
    }

    public class TDRealtimeResponseContainer
    {
        public TDRealtimeResponse[] response { get; set; }
    }

    public class TDRealtimeResponse
    {
        public string service { get; set; }
        public string requestid { get; set; }
        public string command { get; set; }
        public long timestamp { get; set; }
        public TDRealtimeContent content { get; set; }
    }

    public class TDRealtimeContent
    {
        public int code { get; set; }
        public string msg { get; set; }
    }
}
