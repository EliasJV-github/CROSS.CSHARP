using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CROSS.APIREST
{
    public class ApiRest : IApiRest
    {
        private readonly bool dev;


        public ApiRest(IConfiguration configuration)
        {
            dev = configuration.GetValue<bool>("dev");
        }

        public async Task<T> PostAsync<T>(string idOperacion, string url, object request, bool authorization = false, string userName = "", string password = "", List<Tuple<string, string>>? header = default)
        {
            HttpClientHandler clientHandler = new();
            if (dev)
            {
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            }
            HttpClient httpClient = new(clientHandler);
            string _response = string.Empty;
            string myContent = JsonConvert.SerializeObject(request);
            byte[] buffer = Encoding.UTF8.GetBytes(myContent);
            ByteArrayContent byteContent = new(buffer);
            byteContent.Headers.ContentType = new("application/json");
            if (authorization)
            {
                string authenticationString = $"{userName}:{password}";
                string base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
                httpClient.DefaultRequestHeaders.Add($"Authorization", $"Basic {base64EncodedAuthenticationString}");
            }
            if (header != null)
            {
                foreach (Tuple<string, string> item in header)
                {
                    httpClient.DefaultRequestHeaders.Add(item.Item1, item.Item2);
                }
            }
            HttpResponseMessage data = await httpClient.PostAsync(url, byteContent);
            using (HttpContent content = data.Content)
            {
                _response = await content.ReadAsStringAsync();
                var logs = new { url, body = JsonConvert.SerializeObject(request), response = JsonConvert.SerializeObject(_response), statusCode = data.StatusCode };
                Log.Information(string.Format("\n{0}", JsonConvert.SerializeObject(logs)));
            }
            if (data.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(string.Format("No Autorizado en el servicio {0}", url));
            }

            if (data.StatusCode != HttpStatusCode.InternalServerError)
            {
                return JsonConvert.DeserializeObject<T>(_response) ?? throw new FormatException(string.Format("El servicio {0} devuelve un formato invalido", url));
            }
            throw new HttpRequestException(string.Format("Problemas en el servicio {0}", url));
        }

        public async Task<T> GetAsync<T>(string idOperacion, string url, bool authorization = false, string userName = "", string password = "", List<Tuple<string, string>>? header = default)
        {
            HttpClientHandler clientHandler = new();
            if (dev)
            {
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            }
            HttpClient httpClient = new(clientHandler);
            string _response = string.Empty;
            if (authorization)
            {
                string authenticationString = $"{userName}:{password}";
                string base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
                httpClient.DefaultRequestHeaders.Add($"Authorization", $"Basic {base64EncodedAuthenticationString}");
            }
            if (header != null)
            {
                foreach (Tuple<string, string> item in header)
                {
                    httpClient.DefaultRequestHeaders.Add(item.Item1, item.Item2);
                }
            }
            HttpResponseMessage data = await httpClient.GetAsync(url);
            var logs = new { url, body = JsonConvert.SerializeObject(data) };
            Log.Debug(string.Format("CROSS.APIREST => \n {0}", JsonConvert.SerializeObject(logs)));

            using (HttpContent content = data.Content)
            {
                _response = await content.ReadAsStringAsync();
            }
            if (data.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(_response) ?? throw new FormatException(string.Format("El servicio {0} devuelve un formato invalido", url));
            }

            throw new HttpRequestException(string.Format("Problemas en el servicio {0}", url));
        }

        public async Task<T> PostMultipartAsync<T>(string idOperacion, string url, object request, bool authorization = false, string userName = "", string password = "", List<Tuple<string, string>>? header = default)
        {
            HttpClientHandler handler = new();
            if (dev)
            { 
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator; 
            }
            using var httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(300)
            };

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "*/*");
            string _response = string.Empty;
            using var formData = new MultipartFormDataContent();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "*/*");
            if (authorization)
            {
                string authenticationString = $"{userName}:{password}";
                string base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
                httpClient.DefaultRequestHeaders.Add($"Authorization", $"Basic {base64EncodedAuthenticationString}");
            }
            if (header != null)
            {
                foreach (Tuple<string, string> item in header)
                {
                    httpClient.DefaultRequestHeaders.Add(item.Item1, item.Item2);
                }
            }

            foreach (var prop in request.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var value = prop.GetValue(request);
                var name = prop.Name;

                switch (value)
                {
                    case IFormFile file:
                        var fileContent = new StreamContent(file.OpenReadStream());
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                            string.IsNullOrWhiteSpace(file.ContentType)
                                ? "application/octet-stream"
                                : file.ContentType);

                        formData.Add(fileContent, name, Path.GetFileName(file.FileName));
                        break;

                    case System.Collections.IDictionary dict:
                        var json = JsonConvert.SerializeObject(dict);
                        formData.Add(new StringContent(json, Encoding.UTF8, "application/json"), name);
                        break;

                    case string s when name.EndsWith("Json", StringComparison.OrdinalIgnoreCase):
                        var fixedName = name[..^4];
                        formData.Add(new StringContent(s, Encoding.UTF8, "application/json"), fixedName);
                        break;

                    default:
                        formData.Add(new StringContent(value.ToString()!), name);
                        break;
                }
            }

            HttpResponseMessage data = await httpClient.PostAsync(url, formData);
            _response = await data.Content.ReadAsStringAsync();
            if (data.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(string.Format("No Autorizado en el servicio {0}", url));
            }
            if (data.StatusCode == HttpStatusCode.NoContent)
            {
                throw new HttpRequestException("NoContent");
            }

            if (data.StatusCode != HttpStatusCode.InternalServerError)
            {
                return JsonConvert.DeserializeObject<T>(_response) ?? throw new FormatException(string.Format("El servicio {0} devuelve un formato invalido", url));
            }
            throw new HttpRequestException(string.Format("Problemas en el servicio {0}", url));

        }
    }
}
