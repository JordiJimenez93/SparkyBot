using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CoreBot.Services
{
    public class LuisService
    {
        private readonly string _appID;
        private readonly string _appVersion = "0.1";
        private readonly string _authoringKey;
        private readonly string _host = "https://westus.api.cognitive.microsoft.com";
        private readonly string _path;

        public LuisService(IConfiguration configuration)
        {
            _appID = configuration["LuisAppId"];
            _authoringKey = configuration["LuisAPIKey"];
            _path = GetPath();
        }

        private string GetPath()
        {
            return "/luis/api/v2.0/apps/" + _appID + "/versions/" + _appVersion + "/";
        }

        public async Task AddUtterance(string text, string intentName)
        {
            string uri = _host + _path + "examples";
            var body = $@"[{{ ""text"": ""{text}"", ""intentName"": ""{intentName}""}}]";

            var response = await SendPost(uri, body);
            await response.Content.ReadAsStringAsync();
            Console.WriteLine("Added utterances.");
        }

        public async Task Train()
        {
            string uri = _host + _path + "train";

            var response = await SendPost(uri, null);
            await response.Content.ReadAsStringAsync();
            Console.WriteLine("Sent training request.");
        }

        private async Task<HttpResponseMessage> SendPost(string uri, string requestBody)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);

                if (!string.IsNullOrEmpty(requestBody))
                {
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "text/json");
                }

                request.Headers.Add("Ocp-Apim-Subscription-Key", _authoringKey);
                return await client.SendAsync(request);
            }
        }
    }
}
