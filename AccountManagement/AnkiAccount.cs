using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;

namespace AccountManagement
{
    public class AnkiAccountManager
    {
        internal HttpClient _httpClient;

        public SessionData SessionData { get; set; }

        public AnkiAccountManager()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Send a POST to log into the Anki servers with the provided email and password
        /// </summary>
        /// <param name="emailAddress">Email address for the account</param>
        /// <param name="password">Password for the account</param>
        public HttpResponseMessage Login(string emailAddress, string password)
        {
            string uri = "https://accounts.api.anki.com/1/sessions";
            Dictionary<string, string> userAndPass = new Dictionary<string, string>
            {
                ["username"] = emailAddress,
                ["password"] = password
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(userAndPass);
            content.Headers.Add("Anki-App-Key", "luyain9ep5phahP8aph8xa");
            HttpResponseMessage result = _httpClient.PostAsync(uri, content).Result;

            if (result.IsSuccessStatusCode)
            {
                string contentStr = result.Content.ReadAsStringAsync().Result;
                JObject obj = JObject.Parse(contentStr);
                //should be two parts, "session" and "user". We only need session
                SessionData = obj.GetValue("session").ToObject<SessionData>();
            }
            return result;
        }
    }
}
