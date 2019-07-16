using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WaykDen.Models;
using WaykDen.Utils;

namespace WaykDen.Controllers
{
    public class DenRestAPIController
    {
        private string apiKey;
        private string serverUrl;
        public DenRestAPIController(string apikey, string serverUrl)
        {
            this.apiKey = apikey;
            this.serverUrl = serverUrl;
        }

        private async Task<string> Get(string url)
        {
            using(var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.apiKey);
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        private string Post(string url, string value)
        {
            var request = HttpWebRequest.Create(url);
            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", "Bearer " + this.apiKey);
            request.ContentType = "application/json";
            request.Method = "POST";

            try
            {
                if(!string.IsNullOrEmpty(value))
                {
                    var byteData = Encoding.ASCII.GetBytes(value);
                    using(var stream = request.GetRequestStream())
                    {
                        stream.Write(byteData, 0, byteData.Length);
                    }
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            catch(WebException e)
            {
                this.OnError(e);
                return string.Empty;
            }
        }

        private string Put(string url, string value)
        {
            var request = HttpWebRequest.Create(url);
            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", "Bearer " + this.apiKey);
            request.ContentType = "application/json";
            request.Method = "PUT";

            try
            {
                if(!string.IsNullOrEmpty(value))
                {
                    var byteData = Encoding.ASCII.GetBytes(value);
                    using(var stream = request.GetRequestStream())
                    {
                        stream.Write(byteData, 0, byteData.Length);
                    }
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            catch(WebException e)
            {
                this.OnError(e);
                return string.Empty;
            }
        }

        private async Task<string> Delete(string url)
        {
            using(var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("DELETE"), url))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.apiKey);
                    var response = await httpClient.DeleteAsync(url);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetSessions(string parameter = null)
        {
            return await this.Get($"{this.serverUrl}/session{parameter}");
        }

        public async Task<string> GetUsers(string parameter = null)
        {
            return await this.Get($"{this.serverUrl}/user{parameter}");
        }

        public async Task<string> GetLicenses(string parameter = null)
        {
            return await this.Get($"{this.serverUrl}/license/{parameter}");
        }

        public async Task<string> GetConnections(string parameter = null)
        {
            return await this.Get($"{this.serverUrl}/connection{parameter}");
        }

        public string PostLicense(string parameter = null, string content = null)
        {
            return this.Post($"{this.serverUrl}/license/{parameter}", content);
        }

        public string PostUser(string parameter = null, string content = null)
        {
            return this.Post($"{this.serverUrl}/user/{parameter}", content);
        }

        public string PostSession(string parameter = null, string content = null)
        {
            return this.Post($"{this.serverUrl}/session/{parameter}", content);
        }

        public string PutUser(string content, string parameter = null)
        {
            return this.Put($"{this.serverUrl}/user/{parameter}/license", content);
        }

        public async Task<string> DeleteUserLicense(string parameter)
        {
            return await this.Delete($"{this.serverUrl}/user/{parameter}/license");
        }

        public async Task<string> DeleteLicense(string parameter)
        {
            return await this.Delete($"{this.serverUrl}/license/{parameter}");
        }

        public T DeserializeString<T>(string json)
        {
            return (T)JsonConvert.DeserializeObject<T>(json);
        }

        public delegate void OnErrorHandler(Exception e);
        public event OnErrorHandler OnError;
    }
}