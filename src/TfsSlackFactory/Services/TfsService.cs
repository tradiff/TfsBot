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
                return SlackBuildModel.FromEvent(JsonConvert.DeserializeObject<BuildEventHook>(rawEvent));
            }

            return null;
        }


        private async Task<SlackWorkItemModel> GetWorkItem(WorkItemEventHook hookModel)
        {
            string url = $"{_baseAddress}_apis/wit/workItems/{hookModel.Resource.WorkItemId}";
            var tfsWi = await GetWorkItem(url);
            var slackWorkItemModel = SlackWorkItemModel.FromTfs(tfsWi, hookModel);
            if (tfsWi != null && tfsWi.Relations != null && tfsWi.Relations.Any(x => x.Rel == "System.LinkTypes.Hierarchy-Reverse"))
            {
                var parentTfsWi = await GetWorkItem(tfsWi.Relations.Single(x => x.Rel == "System.LinkTypes.Hierarchy-Reverse").Url);
                var parentSlackWi = SlackWorkItemModel.FromTfs(parentTfsWi);
                slackWorkItemModel.ParentWiId = parentSlackWi.WiId;
                slackWorkItemModel.ParentWiTitle = parentSlackWi.WiTitle;
                slackWorkItemModel.ParentWiType = parentSlackWi.WiType;
                slackWorkItemModel.ParentWiUrl = parentSlackWi.WiUrl;
            }
            return slackWorkItemModel;
        }

        private async Task<TfsWorkItemModel> GetWorkItem(string workItemUrl)
        {
            using (var client = GetWebClient())
            {
                var response = await client.GetAsync($"{workItemUrl}?$expand=relations&api-version=1.0");
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var tfsWorkItemModel = JsonConvert.DeserializeObject<TfsWorkItemModel>(responseString);
                    return tfsWorkItemModel;
                }
                else
                {
                    Serilog.Log.Warning($"TFS returned code: {(int)response.StatusCode} {response.StatusCode}");
                    Serilog.Log.Warning(responseString);
                    return null;
                }
            }
        }

        private HttpClient GetWebClient()
        {
            return new HttpClient(new HttpClientHandler { Credentials = _networkCredential });
        }
    }
}