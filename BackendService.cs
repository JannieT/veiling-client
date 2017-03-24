using System;
using System.Net;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Specialized;
using Debug = System.Diagnostics.Debug;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Veiling
{
    public class BackendService
    {
        const string EndPointString = "/api.php";
        const string PingKey = "ping";
        const string BetaalKey = "betaal";
        const string BetaalSoortKey = "betaal_soort";
        const string InfoKey = "bieer_naam";
        const string DetailKey = "bieer_id";
        const string NumberKey = "bieer_id";
        const string PhotoKey = "foto";

        const string UserAgent = "ANDROID APP";

        private string apiEndpoint;
        private string apiKey;

        public BackendService(string endpointDomain, string key) 
        {
            apiEndpoint = endpointDomain + EndPointString;
            apiKey = key; 
        }


        WebHeaderCollection AuthHeader
        {
            get
            {
                var header = new WebHeaderCollection();
                header[HttpRequestHeader.Accept] = "application/json";
                header[HttpRequestHeader.Authorization] = $"Bearer {apiKey}";
                return header;
            }
        }

        public async Task MarkPaid(List<int> nommers, string betaalSoort)
        {
            var payload = new NameValueCollection();
            payload.Set(BetaalKey, JsonConvert.SerializeObject(nommers));
            payload.Set(BetaalSoortKey, betaalSoort);

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers = AuthHeader;

                    Debug.WriteLine ($"Updating {this.apiEndpoint} ...");
                    byte[] response = await client.UploadValuesTaskAsync(this.apiEndpoint, "POST", payload);
                    Debug.WriteLine("Response: " + Encoding.UTF8.GetString(response));
                }
                catch (WebException ex)
                {
                    throw MapException(ex);
                }
            }
        }


        public async Task PingAsync()
        {
            Debug.WriteLine("PingAsync");
            using (var client = new WebClient())
            {
                try
                {
                    client.Headers = AuthHeader;
                
                    string response = await client.DownloadStringTaskAsync($"{apiEndpoint}?{PingKey}");
                    //Debug.WriteLine($"ping response: {response}");
                }
                catch (WebException ex)
                {
                    throw MapException(ex);
                }
            }
        }

		public async Task<BieerItem> PullBieerInfoAsync(int id)
		{
			string json = "";
			using (var client = new WebClient ()) {
                try
                {
                    client.Headers = AuthHeader;

                    json = await client.DownloadStringTaskAsync($"{apiEndpoint}?{InfoKey}={id}");
                    Debug.WriteLine($"response: {json}");
                }
                catch (WebException ex)
                {
                    throw MapException(ex);
                }
			}

			BieerItem bieer = JsonConvert.DeserializeObject<BieerItem> (json);
            // TODO: endpoint should ideally return this
            bieer.id = id;
            bieer.nommer = $"{id}";

			return bieer;
		}

        public async Task<List<VeilingItem>> PullItemsAsync(int id)
        {
            string jsonItems = "[]";

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers = AuthHeader;

                    jsonItems = await client.DownloadStringTaskAsync($"{apiEndpoint}?{DetailKey}={id}");
                    Debug.WriteLine($"jsonItems: {jsonItems}");
                }
                catch (WebException ex)
                {
                    throw MapException(ex);
                }
            }

            var list = new List<VeilingItem>();
            JArray items = JArray.Parse(jsonItems);
            list = items.Select(item => new VeilingItem(item)).ToList();

            return list;
        }



        public Task UploadImageAsync(int number, string imageFile)
        {
            return Task.Run(() =>
            {
            
                using (var client = new WebClient())
                {
                    client.Headers = AuthHeader;
                    try
                    {
                        Debug.WriteLine($"upload to {apiEndpoint}");
                        client.UploadFile(new Uri($"{apiEndpoint}"), "POST", imageFile);
                    }
                    catch (WebException ex)
                    {
                        throw MapException(ex);
                    }
                }
            });
        }


        Exception MapException(WebException ex)
        {
            Debug.WriteLine($"Status code: {((HttpWebResponse)ex.Response).StatusCode}");
            Debug.WriteLine($"message: {ex.Message}");

            switch (ex.Status)
            {
                case WebExceptionStatus.NameResolutionFailure:
                    return new NoNetworkException();

                case WebExceptionStatus.ProtocolError:
                    var code = ((HttpWebResponse)ex.Response).StatusCode;
                    Debug.WriteLine($"Code: {code}");

                    if (code == HttpStatusCode.Unauthorized)
                    {
                        return new AuthenticationException();
                    }

                    if (code == HttpStatusCode.NotFound)
                    {
                        return new ItemNotFoundException();
                    }

                    if (code == HttpStatusCode.Forbidden)
                    {
                        return new ForbiddenException();
                    }

                    if (code == (HttpStatusCode)422)
                    {
                        return new ValidationException();
                    }

                    return new WebException("Cloud error", ex);

                default:
                    return new WebException("Cloud error", ex);
            }
        }
    }
}

