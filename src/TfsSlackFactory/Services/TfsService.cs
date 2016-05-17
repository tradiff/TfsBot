using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TfsSlackFactory.Models;

namespace TfsSlackFactory.Services
{
    public class TfsService
    {
        private readonly NetworkCredential _networkCredential;
        private readonly string _baseAddress;

        public TfsService(IOptions<TfsSettings> tfsSettings)
        {
            _networkCredential = new NetworkCredential(tfsSettings.Value.Username, tfsSettings.Value.Password);
            _baseAddress = tfsSettings.Value.Server;
        }


        public dynamic GetWorkItem(int workItemId)
        {
            using (WebClient client = GetWebClient())
            {
                var response = client.DownloadString($"{_baseAddress}_apis/wit/workItems/{workItemId}?$expand=relations&api-version=1.0");
                dynamic responseObject = JObject.Parse(response);
                return responseObject;
            }
        }

        public bool IsWorkItemInQuery(int workItemId, string project, string query)
        {
            if (query.Contains("@wiId"))
            {
                query = query.Replace("@wiId", workItemId.ToString());
            }

            var requestUrl = $"{_baseAddress}{project}/_apis/wit/wiql?api-version=1.0";
            using (WebClient client = GetWebClient())
            {
                client.Headers.Add("Content-Type", "application/json");
                var jsonBody = JsonConvert.SerializeObject(new { query });
                var response = client.UploadData(requestUrl, "POST", Encoding.UTF8.GetBytes(jsonBody));
                dynamic responseObject = JObject.Parse(Encoding.UTF8.GetString(response));
                
                foreach (var workItem in responseObject.workItems)
                {
                    if (workItem.id == workItemId)
                    {
                        return true;
                    }    
                }
                return false;
            }
        }
        
        private WebClient GetWebClient()
        {
            return new WebClient {Credentials = _networkCredential};
        }
    }
}