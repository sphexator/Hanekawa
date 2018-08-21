using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hanekawa.Rest
{
    public class RestClient : IDisposable
    {
        private readonly HttpClient _client;
        private readonly bool _ensureSuccess;

        public RestClient(string base_address, bool ensureSuccess = false)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(base_address);
            _ensureSuccess = ensureSuccess;
        }

        public HttpRequestHeaders Headers => _client.DefaultRequestHeaders;

        public void Dispose()
        {
            _client.Dispose();
        }

        public event Action<string, string> OnRequestComplete;

        public RestClient AddHeader(string header, string value)
        {
            _client.DefaultRequestHeaders.Add(header, value);
            return this;
        }

        public async Task<RestResponse> DeleteAsync(string url = "")
        {
            return await SendAsync(RestMethod.DELETE, url);
        }

        public async Task<RestResponse> GetAsync(string url = "")
        {
            return await SendAsync(RestMethod.GET, url);
        }

        public async Task<RestResponse<T>> GetAsync<T>(string url = "")
        {
            return await SendAsync<T>(RestMethod.GET, url);
        }

        public async Task<Stream> GetStreamAsync(string url = "")
        {
            var response = await GetAsync(url);
            return await response.HttpResponseMessage.Content.ReadAsStreamAsync();
        }

        public async Task<RestResponse> PostAsync(string url = "", string value = null)
        {
            return await SendAsync(RestMethod.POST, url, value);
        }

        public async Task<RestResponse<T>> PostAsync<T>(string url = "", string value = null)
        {
            var response = new RestResponse<T>(await PostAsync(url, value));
            response.Data = JsonConvert.DeserializeObject<T>(response.Body);
            return response;
        }

        public async Task<RestResponse> PatchAsync(string url = "", string value = null)
        {
            return await SendAsync(RestMethod.PATCH, url, value);
        }

        public async Task<RestResponse<T>> PatchAsync<T>(string url = "", string value = null)
        {
            var r = new RestResponse<T>(await PatchAsync(url, value));
            r.Data = JsonConvert.DeserializeObject<T>(r.Body);
            return r;
        }

        public async Task<RestResponse> PutAsync(string url = "", string value = null)
        {
            return await SendAsync(RestMethod.PUT, url, value);
        }

        public async Task<RestResponse<T>> PutAsync<T>(string url = "", string value = null)
        {
            var r = new RestResponse<T>(await PutAsync(url, value));
            r.Data = JsonConvert.DeserializeObject<T>(r.Body);
            return r;
        }

        public async Task<RestResponse> PostMultipartAsync(string url = "", params MultiformItem[] items)
        {
            var req = new HttpRequestMessage(new HttpMethod("POST"), _client.BaseAddress + url);

            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

            req.Headers.Add("Connection", "keep-alive");
            req.Headers.Add("Keep-Alive", "600");

            var content = new MultipartFormDataContent(boundary);
            if (items != null && items.Any())
                foreach (var kvp in items)
                    content.Add(kvp.Content, kvp.Name);

            req.Content = content;

            return await SendAsync(req);
        }

        public RestClient SetAuthorization(string key)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(key);
            return this;
        }

        public RestClient SetAuthorization(string scheme, string value)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, value);
            return this;
        }

        public async Task<RestResponse> SendAsync(RestMethod method, string url = null, string value = null)
        {
            url = url.TrimStart('/');

            var m = new HttpMethod(method.ToString().ToUpper());
            using (var msg = new HttpRequestMessage(m, url))
            {
                if (!string.IsNullOrWhiteSpace(value))
                    msg.Content = new StringContent(value, Encoding.UTF8, "application/json");

                return await SendAsync(msg);
            }
        }

        public async Task<RestResponse> SendAsync(HttpRequestMessage message)
        {
            var response = await _client.SendAsync(message);

            var restResponse = new RestResponse();
            restResponse.HttpResponseMessage = response;
            restResponse.Body = await response.Content.ReadAsStringAsync();
            restResponse.Success = response.IsSuccessStatusCode;

            if (restResponse.Success)
                OnRequestComplete?.Invoke(
                    message.Method.Method,
                    message.RequestUri.AbsolutePath
                );

            if (_ensureSuccess) response.EnsureSuccessStatusCode();

            return restResponse;
        }

        public async Task<RestResponse<T>> SendAsync<T>(RestMethod method, string url, string value = null)
        {
            var response = new RestResponse<T>(await SendAsync(method, url, value));
            response.Data = JsonConvert.DeserializeObject<T>(response.Body);
            return response;
        }

        public async Task<RestResponse<T>> SendAsync<T>(HttpRequestMessage message)
        {
            var response = new RestResponse<T>(await SendAsync(message));
            response.Data = JsonConvert.DeserializeObject<T>(response.Body);
            return response;
        }
    }

    public class MultiformItem
    {
        public HttpContent Content { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; } = null;
    }
}