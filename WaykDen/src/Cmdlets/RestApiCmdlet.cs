using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using WaykDen.Controllers;
using WaykDen.Utils;

namespace WaykDen.Cmdlets
{
    public class RestApiCmdlet : BaseCmdlet
    {
        private const string DEN_API_KEY_ENV = "DEN_API_KEY";

        private const string DEN_ACCESS_TOKEN = "DEN_ACCESS_TOKEN";

        private const string DEN_REFRESH_TOKEN = "DEN_REFRESH_TOKEN";

        private const string DEN_SERVER_URL_ENV = "DEN_SERVER_URL";

        private const string API_KEY_FIELD = "ApiKey";

        private const string SERVER_URL_FIELD = "Server URL";

        [Parameter(HelpMessage = "WaykDen server API key.")]
        public string ApiKey {get; set;} = string.Empty;

        [Parameter(HelpMessage = "WaykDen server external URL.")]
        public string ServerUrl {get; set;} = string.Empty;

        protected DenRestAPIController DenRestAPIController {get; set;}

        protected override void BeginProcessing()
        {
            if (string.IsNullOrEmpty(this.ServerUrl))
            {
                this.ServerUrl = Environment.GetEnvironmentVariable(DEN_SERVER_URL_ENV);
            }

            if (string.IsNullOrEmpty(this.ApiKey))
            {
                string refreshToken = Environment.GetEnvironmentVariable(DEN_REFRESH_TOKEN);
                string accessToken = Environment.GetEnvironmentVariable(DEN_ACCESS_TOKEN);

                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
                {
                    string[] base64Strings = accessToken.Split('.');
                    if (base64Strings.Length > 0)
                    {
                        string base64 = base64Strings[1];
                        var base64EncodedBytes = Base64Url.Decode(base64);
                        string json = Encoding.UTF8.GetString(base64EncodedBytes);
                        AccessTokenDecodedObject access = JsonConvert.DeserializeObject<AccessTokenDecodedObject>(json);
                        DateTime expirationDate = DateTimeOffset.FromUnixTimeSeconds(access.exp).DateTime;

                        if ((expirationDate - DateTime.UtcNow).TotalSeconds < 30)
                        {
                            WebRequest request = WebRequest.Create(this.ServerUrl + "/.well-known/configuration");
                            request.ContentType = "application/json";
                            request.Method = "GET";

                            WebResponse response = request.GetResponse();
                            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                            LucidObject lucidResult = JsonConvert.DeserializeObject<LucidObject>(responseString);

                            var dict = new Dictionary<string, string>
                            {
                                { "grant_type", "refresh_token" },
                                { "refresh_token", refreshToken },
                                { "client_id", lucidResult.wayk_client_id }
                            };

                            using (HttpClient client = new HttpClient())
                            {
                                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, lucidResult.lucid_uri + "/auth/token") { Content = new FormUrlEncodedContent(dict) };
                                HttpResponseMessage res = client.SendAsync(req).Result;

                                AccessTokenObject accessTokenObject = JsonConvert.DeserializeObject<AccessTokenObject>(res.Content.ReadAsStringAsync().Result);

                                Environment.SetEnvironmentVariable(DEN_ACCESS_TOKEN, accessTokenObject.access_token);
                                Environment.SetEnvironmentVariable(DEN_REFRESH_TOKEN, accessTokenObject.refresh_token);
                                this.ApiKey = accessTokenObject.access_token;
                            }
                        }
                        else
                        {
                            this.ApiKey = accessToken;
                        }
                    }
                    else
                    {
                        this.OnError(new Exception("Error on the Access Token, please reconnect your den user"));
                    }
                }
                else
                {
                    this.ApiKey = Environment.GetEnvironmentVariable(DEN_API_KEY_ENV);
                }
            }

            if(string.IsNullOrEmpty(this.ApiKey) || string.IsNullOrEmpty(this.ServerUrl))
            {
                Dictionary<string, PSObject> prompt =  this.Host.UI.Prompt
                (
                    "WaykDen cmdlet",
                    "Supply values for the following parameters:",
                    new Collection<FieldDescription>(){new FieldDescription(SERVER_URL_FIELD), new FieldDescription(API_KEY_FIELD)}
                );

                bool okApiKey = prompt.TryGetValue(API_KEY_FIELD, out PSObject objApi);
                bool okServerUrl = prompt.TryGetValue(SERVER_URL_FIELD, out PSObject objUrl);

                this.ApiKey = (string)objApi.BaseObject;
                this.ServerUrl = (string)objUrl.BaseObject;
            }

            if(string.IsNullOrEmpty(this.ApiKey) || string.IsNullOrEmpty(this.ServerUrl))
            {
                this.OnError(new Exception("No API key or server URL were provided"));
            }

            this.ServerUrl = this.ServerUrl.TrimEnd('/');
            this.DenRestAPIController = new DenRestAPIController(this.ApiKey, this.ServerUrl);
            this.DenRestAPIController.OnError += this.OnError;
        }

        private class LucidObject
        {
            public string wayk_client_id { get; set; }

            public string lucid_uri { get; set; }
        }

        private class AccessTokenObject
        {
            public string access_token { get; set; }

            public string refresh_token { get; set; }
        }

        private class AccessTokenDecodedObject
        {
            public long exp { get; set; }
        }
    }
}
