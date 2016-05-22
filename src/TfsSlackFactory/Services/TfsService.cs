using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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

        public async Task<ITfsEvent> CreateEventObject(string rawEvent)
        {
            var regex = new Regex(@"""eventType"".+?""(.+?)""");
            var eventType = regex.Match(rawEvent).Groups[1].Value;

            if (eventType.StartsWith("workitem"))
            {
                return await GetWorkItem(JsonConvert.DeserializeObject<WorkItemEventHook>(rawEvent));

            }
            if (eventType.StartsWith("build"))
            {
                return JsonConvert.DeserializeObject<BuildEventHook>(rawEvent);
            }

            return null;
        }


        private async Task<SlackWorkItemModel> GetWorkItem(WorkItemEventHook hookModel)
        {
            using (var client = GetWebClient())
            {
                var response = await client.GetAsync($"{_baseAddress}_apis/wit/workItems/{hookModel.Resource.WorkItemId}?$expand=relations&api-version=1.0");
                var obj = JsonConvert.DeserializeObject<TfsWorkItemModel>(await response.Content.ReadAsStringAsync());

                var model = SlackWorkItemModel.FromTfs(obj, hookModel);

                if (obj.Relations != null && obj.Relations.Any(x => x.Rel == "System.LinkTypes.Hierarchy-Reverse"))
                {
                    var parentModel = await GetWorkItem(obj.Relations.Single(x => x.Rel == "System.LinkTypes.Hierarchy-Reverse").Url);
                    model.ParentWiId = parentModel.WiId;
                    model.ParentWiTitle = parentModel.WiTitle;
                    model.ParentWiType = parentModel.WiType;
                    model.ParentWiUrl = parentModel.WiUrl;
                }

                return model;
            }
        }

        private async Task<SlackWorkItemModel> GetWorkItem(string workItemUrl)
        {
            using (var client = GetWebClient())
            {
                var response = await client.GetAsync($"{workItemUrl}?$expand=relations&api-version=1.0");
                var obj = JsonConvert.DeserializeObject<TfsWorkItemModel>(await response.Content.ReadAsStringAsync());

                return SlackWorkItemModel.FromTfs(obj);
            }
        }

        private HttpClient GetWebClient()
        {
            return new HttpClient(new HttpClientHandler { Credentials = _networkCredential });
        }
    }
}