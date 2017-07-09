using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FoE.Farmer.Library
{
    public class Payload
    {
        private static int _requestId = 0;
        public string ClassName { get; set; } = "ServerRequest";
        public string RequestClass { get; set; }
        public JArray RequestData { get; set; } = new JArray();
        private static int requestId => _requestId++;
        public string RequestMethod { get; set; }
        public int RequestId { get; private set; }
        public Task<object> Task => TaskSource.Task;

        public TaskCompletionSource<object> TaskSource = new TaskCompletionSource<object>();

        public Payload()
        {
            RequestId = requestId;
        }

        public JObject ToJsonObject()
        {
            var j = new JObject();
            j["requestClass"] = RequestClass;
            j["requestData"] = new JArray(RequestData);
            j["requestId"] = RequestId;
            j["requestMethod"] = RequestMethod;
            j["voClassName"] = ClassName;
            j["__class__"] = ClassName;

            return j;
        }

        public void Cancel()
        {
            TaskSource.SetCanceled();
        }


        public override string ToString()
        {
            return ToJsonObject().ToString(Formatting.None);
        }
    }
}
