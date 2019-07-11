using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using WaykPS.Config;
using WaykPS.Cmdlets;

namespace WaykPS.Controllers
{
    public class DenRestAPIController
    {
        private const string WAYK_DEN_HOME = "WAYK_DEN_HOME";
        public string Path {get; set;} = string.Empty;
        private DenConfigStore store;
        public DenConfig DenConfig {get;}
        public DenRestAPIController(string path)
        {
            this.Path = Environment.GetEnvironmentVariable(WAYK_DEN_HOME);
            if(string.IsNullOrEmpty(this.Path))
            {
                this.Path = path;
            }

            this.Path = this.Path.EndsWith($"{System.IO.Path.DirectorySeparatorChar}") ? this.Path : $"{this.Path}{System.IO.Path.DirectorySeparatorChar}";
            try
            {
                this.store = new DenConfigStore($"{this.Path}WaykDen.db");
                this.DenConfig = new DenConfig(){DenConfigs = this.store.GetConfig(), Path = this.Path};
                
            }
            catch(Exception e)
            {
                if(this.OnError != null) this.OnError(e);
            }
        }

        private async Task<string> Get(string url)
        {
            using(var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.DenConfig.DenServerConfigObject.ApiKey);
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        private string Post(string url, string value)
        {
            var request = HttpWebRequest.Create(url);
            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", "Bearer " + this.DenConfig.DenServerConfigObject.ApiKey);
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
            request.Headers.Add("Authorization", "Bearer " + this.DenConfig.DenServerConfigObject.ApiKey);
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
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.DenConfig.DenServerConfigObject.ApiKey);
                    var response = await httpClient.DeleteAsync(url);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetSessions(string parameter = null)
        {
            return await this.Get($"{this.DenConfig.DenServerConfigObject.ExternalUrl}/session{parameter}");
        }

        public async Task<string> GetUsers(string parameter = null)
        {
            return await this.Get($"{this.DenConfig.DenServerConfigObject.ExternalUrl}/user{parameter}");
        }

        public async Task<string> GetLicenses(string parameter = null)
        {
            return await this.Get($"{this.DenConfig.DenServerConfigObject.ExternalUrl}/license{parameter}");
        }

        public async Task<string> GetConnections(string parameter = null)
        {
            return await this.Get($"{this.DenConfig.DenServerConfigObject.ExternalUrl}/connection{parameter}");
        }

        public string PostLicense(string parameter = null, string content = null)
        {
            return this.Post($"{this.DenConfig.DenServerConfigObject.ExternalUrl}/license/{parameter}", content);
        }

        public string PostUser(string parameter = null, string content = null)
        {
            return this.Post($"{this.DenConfig.DenServerConfigObject.ExternalUrl}/user/{parameter}", content);
        }

        public string PostSession(string parameter = null, string content = null)
        {
            return this.Post($"{this.DenConfig.DenServerConfigObject.ExternalUrl}/session/{parameter}", content);
        }

        public string PutUser(string content, string parameter = null)
        {
            return this.Put($"{this.DenConfig.DenServerConfigObject.ExternalUrl}/user{parameter}/license", content);
        }

        public async Task<string> DeleteUserLicense(string parameter)
        {
            return await this.Delete($"{this.DenConfig.DenServerConfigObject.ExternalUrl}/user/{parameter}/license");
        }

        public async Task<string> DeleteLicense(string parameter)
        {
            return await this.Delete($"{this.DenConfig.DenServerConfigObject.ExternalUrl}/license/{parameter}");
        }

        public T DeserializeString<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public delegate void OnErrorHandler(Exception e);
        public event OnErrorHandler OnError;
    }
}