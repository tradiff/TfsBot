using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Options;
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


        public SlackWorkItemModel GetWorkItem(int workItemId)
        {
            using (var client = GetWebClient())
            {
                var response = client.GetAsync($"{_baseAddress}_apis/wit/workItems/{workItemId}?$expand=relations&api-version=1.0").Result;
                var obj = JsonConvert.DeserializeObject<TfsWorkItemModel>(response.Content.ReadAsStringAsync().Result);

                var model = SlackWorkItemModel.FromTfs(obj);

                if (obj.Relations != null && obj.Relations.Any(x => x.Rel == "System.LinkTypes.Hierarchy-Reverse"))
                {
                    var parentModel = GetWorkItem(obj.Relations.Single(x => x.Rel == "System.LinkTypes.Hierarchy-Reverse").Url);
                    model.ParentWiId = parentModel.WiId;
                    model.ParentWiTitle = parentModel.WiTitle;
                    model.ParentWiType = parentModel.WiType;
                    model.ParentWiUrl = parentModel.WiUrl;
                }

                return model;
            }
        }

        private SlackWorkItemModel GetWorkItem(string workItemUrl)
        {
            using (var client = GetWebClient())
            {
                var response = client.GetAsync($"{workItemUrl}?$expand=relations&api-version=1.0").Result;
                var obj = JsonConvert.DeserializeObject<TfsWorkItemModel>(response.Content.ReadAsStringAsync().Result);

                return SlackWorkItemModel.FromTfs(obj);
            }
        }

        public bool IsWorkItemInQuery(int workItemId, string project, string query)
        {
            if (query.Contains("@wiId"))
            {
                query = query.Replace("@wiId", workItemId.ToString());
            }

            var requestUrl = $"{_baseAddress}{project}/_apis/wit/wiql?api-version=1.0";
            using (var client = GetWebClient())
            {
                var jsonBody = JsonConvert.SerializeObject(new { query });

                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = client.PostAsync(requestUrl, content).Result;
                dynamic responseObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                if (responseObject.workItemRelations != null)
                {
                    foreach (var workItem in responseObject.workItemRelations)
                    {
                        if (workItem.source?.id == workItemId)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                if (responseObject.workItems != null)
                {
                    foreach (var workItem in responseObject.workItems)
                    {
                        if (workItem.id == workItemId)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private HttpClient GetWebClient()
        {
            return new HttpClient(new HttpClientHandler { Credentials = _networkCredential });
        }
    }
}