using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Hanekawa.Rest
{
    public class RestResponse
    {
        public string Body { get; internal set; }
        public HttpResponseMessage HttpResponseMessage { get; internal set; }
        public bool Success { get; internal set; }
    }

    public class RestResponse<T> : RestResponse
    {
        public RestResponse() { }
        public RestResponse(RestResponse r)
        {
            Body = r.Body;
            HttpResponseMessage = r.HttpResponseMessage;
            Success = r.Success;
        }

        public T Data { get; internal set; }
    }
}
