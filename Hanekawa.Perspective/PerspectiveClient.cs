using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Hanekawa.Perspective.Models;
using Newtonsoft.Json;

namespace Hanekawa.Perspective
{
    public class PerspectiveClient
    {
        private const string URL = "https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze?key=";

        public async Task<AnalyzeCommentResponse> GetToxicityScore(string content, string key)
        {
            using (var client = new HttpClient())
            {
                var request = new StringContent(JsonConvert.SerializeObject(new AnalyzeCommentRequest(content)),
                    Encoding.UTF8, "application/json");
                var response = (await client.PostAsync(URL + key, request)).EnsureSuccessStatusCode();
                var data = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<AnalyzeCommentResponse>(data);
                return result;
            }
        }
    }
}