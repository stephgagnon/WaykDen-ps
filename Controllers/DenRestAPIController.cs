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
        private const string ENCODED_PLUS = "%2B";
        private string apiKey;
        private string serverUrl;
        public DenRestAPIController(string apikey, string serverUrl)
        {
            this.apiKey = apikey;
            this.serverUrl = serverUrl;
        }

        private async Task<string> Get(string url)
        {
            url = url.Replace("+", ENCODED_PLUS);
            this.ValidateUrl(url);
            using(var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.apiKey);
                    var response = await httpClient.SendAsync(request);
                    if(response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"Error response is {response.StatusCode}");
                    }
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        private string Post(string url, string value)
        {
            this.ValidateUrl(url);
            var request = HttpWebRequest.Create(url);
            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", "Bearer " + this.apiKey);
            request.ContentType = "application/json";
            request.Method = "POST";

            try
            {
                if(!string.IsNullOrEmpty(value))
                {
                    var byteData = Encoding.UTF8.GetBytes(value);
                    using(var stream = request.GetRequestStream())
                    {
                        stream.Write(byteData, 0, byteData.Length);
                    }
                }

                var response = (HttpWebResponse)request.GetResponse();
                if(response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Error response is {response.StatusCode}");
                }
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
            this.ValidateUrl(url);
            var request = HttpWebRequest.Create(url);
            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", "Bearer " + this.apiKey);
            request.ContentType = "application/json";
            request.Method = "PUT";

            try
            {
                if(!string.IsNullOrEmpty(value))
                {
                    var byteData = Encoding.UTF8.GetBytes(value);
                    using(var stream = request.GetRequestStream())
                    {
                        stream.Write(byteData, 0, byteData.Length);
                    }
                }

                var response = (HttpWebResponse)request.GetResponse();
                if(response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Error response is {response.StatusCode}");
                }
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            catch(WebException e)
            {
                this.OnError(e);
                return string.Empty;
            }
        }

        private async Task<string> Delete(string url, string value = null)
        {
            this.ValidateUrl(url);
            var request = WebRequest.Create(url);
            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", "Bearer " + this.apiKey);
            request.ContentType = "application/json";
            request.Method = "DELETE";

            try
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var byteData = Encoding.UTF8.GetBytes(value);
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(byteData, 0, byteData.Length);
                    }
                }

                var response = (HttpWebResponse) await request.GetResponseAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Error response is {response.StatusCode}");
                }
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            catch (WebException e)
            {
                this.OnError(e);
                return string.Empty;
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

        public async Task<string> GetGroups(string parameter = null)
        {
            return await this.Get($"{this.serverUrl}/group/{parameter}");
        }

        public async Task<string> GetUserFromGroup(string groupID, string parameter = null)
        {
            return await this.Get($"{this.serverUrl}/group/{groupID}/user{parameter}");
        }

        public async Task<string> GetRoles(string parameter = null)
        {
            return await this.Get($"{this.serverUrl}/role/{parameter}");
        }

        public async Task<string> GetLicenses(string parameter = null)
        {
            return await this.Get($"{this.serverUrl}/license/{parameter}");
        }

        public async Task<string> GetConnections(string parameter = null)
        {
            return await this.Get($"{this.serverUrl}/connection{parameter}");
        }

        public string PostCreateUser(string content)
        {
            return this.Post($"{this.serverUrl}/user", content);
        }

        public string PostCreateGroup(string content)
        {
            return this.Post($"{this.serverUrl}/group", content);
        }

        public string PostCreateRole(string content)
        {
            return this.Post($"{this.serverUrl}/role", content);
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

        public string PutUser(string content)
        {
            return this.Put($"{this.serverUrl}/user/license", content);
        }

        public string PutUserToGroup(string content, string groupID)
        {
            return this.Put($"{this.serverUrl}/group/{groupID}/user", content);
        }

        public string PutRoleToGroup(string content, string groupID)
        {
            return this.Put($"{this.serverUrl}/group/{groupID}/role", content);
        }

        public string PutSetUserUpdate(string content)
        {
            return this.Put($"{this.serverUrl}/user/", content);
        }

        public string PutSetUserPassword(string content, string UserId)
        {
            return this.Put($"{this.serverUrl}/user/{UserId}/password", content);
        }

        public string PutRoleMember(string content, string userID)
        {
            return this.Put($"{this.serverUrl}/user/{userID}/role", content);
        }

        public async Task<string> DeleteGroup(string parameter)
        {
            return await this.Delete($"{this.serverUrl}/group/{parameter}");
        }

        public async Task<string> DeleteUserFromGroup(string groupID, string content)
        {
            return await this.Delete($"{this.serverUrl}/group/{groupID}/user", content);
        }

        public async Task<string> DeleteRole(string parameter)
        {
            return await this.Delete($"{this.serverUrl}/role/{parameter}");
        }

        public async Task<string> DeleteUser(string parameter)
        {
            return await this.Delete($"{this.serverUrl}/user/{parameter}");
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

        private void ValidateUrl(string url)
        {
            Uri uriResult;
            bool valid = Uri.TryCreate(url, UriKind.Absolute, out uriResult);
            if(!valid)
            {
                throw new Exception("Invalid URL.");
            }
        }

        public delegate void OnErrorHandler(Exception e);
        public event OnErrorHandler OnError;
    }
}