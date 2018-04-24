using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TfsBot.Models;

namespace TfsBot.Services
{
    public class TfsService
    {
        private readonly SettingsModel _settings;
        private readonly NetworkCredential _networkCredential;
        private readonly string _baseAddress;

        public TfsService(IOptions<SettingsModel> settings)
        {
            _settings = settings.Value;
            _networkCredential = new NetworkCredential(_settings.Tfs.Username, _settings.Tfs.Password);

            _baseAddress = _settings.Tfs.Server;
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

        public async void SetupSubscriptions()
        {
            var existingSubscriptions = await GetSubscriptions();

            foreach (var subscription in existingSubscriptions)
            {
                if (!_settings.IntegrationGroups.Any(x => x.Name == subscription.integrationName))
                {
                    // if integration name is not valid
                    DeleteSubscription(subscription.id);
                }
            }

            foreach (var integrationGroup in _settings.IntegrationGroups)
            {
                var existingSubscription = existingSubscriptions.Find(x => x.integrationName == integrationGroup.Name);
                if (existingSubscription == null)
                {
                    CreateSubscription(integrationGroup);
                }
                else
                {
                    UpdateSubscription(integrationGroup, existingSubscription.id);
                }
            }
        }

        private async Task<List<TfsSubscriptionModel>> GetSubscriptions()
        {
            using (var client = GetWebClient())
            {
                string url = $"{_baseAddress}_apis/hooks/subscriptions/?api-version=1.0";
                var response = await client.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var tfsSubscriptions = JsonConvert.DeserializeObject<TfsSubscriptionListModel>(responseString);
                    var filteredSubscriptions = tfsSubscriptions.value.Where(x =>
                        x?.consumerInputs?.httpHeaders != null &&
                        x.consumerInputs.httpHeaders.Contains($"x-created-by:{_settings.SelfName ?? "TfsBot"}")
                        ).ToList();

                    return filteredSubscriptions;
                }
                else
                {
                    Serilog.Log.Warning($"TFS returned code: {(int)response.StatusCode} {response.StatusCode}");
                    Serilog.Log.Warning(responseString);
                    return null;
                }
            }
        }

        private async void CreateSubscription(SettingsIntegrationGroupModel integrationGroup)
        {
            using (var client = GetWebClient())
            {
                var postDataString = await GetPostDataForIntegration(integrationGroup);

                var response = await client.PostAsync($"{_baseAddress}_apis/hooks/subscriptions?api-version=1.0",
                    new StringContent(postDataString, Encoding.UTF8, "application/json"));

                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    Serilog.Log.Information($"Created subscription for {integrationGroup.Name}");
                }
                else
                {
                    Serilog.Log.Warning($"TFS returned code: {(int)response.StatusCode} {response.StatusCode} while creating subscription for {integrationGroup.Name}");
                    Serilog.Log.Warning(responseString);
                }
            }
        }

        private async void UpdateSubscription(SettingsIntegrationGroupModel integrationGroup, string subscriptionId)
        {
            using (var client = GetWebClient())
            {
                var postDataString = await GetPostDataForIntegration(integrationGroup);
                var response = await client.PutAsync($"{_baseAddress}_apis/hooks/subscriptions/{subscriptionId}?api-version=1.0",
                    new StringContent(postDataString, Encoding.UTF8, "application/json"));

                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    Serilog.Log.Information($"Updated subscription for {integrationGroup.Name}");
                }
                else
                {
                    Serilog.Log.Warning($"TFS returned code: {(int)response.StatusCode} {response.StatusCode} while updating subscription for {integrationGroup.Name}");
                    Serilog.Log.Warning(responseString);
                }
            }
        }

        private async Task<string> GetPostDataForIntegration(SettingsIntegrationGroupModel integrationGroup)
        {
            var projectId = await GetProjectIdFromName(integrationGroup.TfsProject);
            var postData = new
            {
                publisherId = "tfs",
                eventType = integrationGroup.EventType,
                consumerId = "webHooks",
                consumerActionId = "httpRequest",
                publisherInputs = new
                {
                    projectId = projectId
                },
                consumerInputs = new
                {
                    resourceDetailsToSend = "All",
                    detailedMessagesToSend = "none",
                    messagesToSend = "none",
                    url = $"{_settings.SelfUrl}api/webhook/?integration={integrationGroup.Name}",
                    httpHeaders = $"x-created-by:{_settings.SelfName ?? "TfsBot"}"
                }
            };
            var postDataString = JsonConvert.SerializeObject(postData);
            return postDataString;
        }

        private async void DeleteSubscription(string id)
        {
            using (var client = GetWebClient())
            {
                var response = await client.DeleteAsync($"{_baseAddress}_apis/hooks/subscriptions/{id}?api-version=1.0");
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    Serilog.Log.Information($"Deleted subscription {id}");
                }
                else
                {
                    Serilog.Log.Warning($"TFS returned code: {(int)response.StatusCode} {response.StatusCode} while deleting subscription {id}");
                    Serilog.Log.Warning(responseString);
                }
            }
        }

        private async Task<string> GetProjectIdFromName(string projectName)
        {

            using (var client = GetWebClient())
            {
                string url = $"{_baseAddress}_apis/projects/{projectName}?api-version=1.0";
                var response = await client.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var projectDefinition = new { id = "" };
                    var project = JsonConvert.DeserializeAnonymousType(responseString, projectDefinition);
                    return project.id;
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