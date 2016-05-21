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


        public SlackWorkItemModel GetWorkItem(WorkItemHook hookModel)
        {
            using (var client = GetWebClient())
            {
                var response = client.GetAsync($"{_baseAddress}_apis/wit/workItems/{hookModel.Resource.WorkItemId}?$expand=relations&api-version=1.0").Result;
                var obj = JsonConvert.DeserializeObject<TfsWorkItemModel>(response.Content.ReadAsStringAsync().Result);

                var model = SlackWorkItemModel.FromTfs(obj, hookModel);

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

        private HttpClient GetWebClient()
        {
            return new HttpClient(new HttpClientHandler { Credentials = _networkCredential });
        }
    }
}