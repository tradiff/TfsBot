using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TfsBot.Models;

namespace TfsBot.Services
{
    public class TfsService
    {
        private static HttpClient _client;
        private readonly SettingsModel _settings;
        private readonly string _baseAddress;

        public TfsService(IOptions<SettingsModel> settings)
        {
            _settings = settings.Value;

            if (_client == null)
            {
                string credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($":{_settings.Tfs.PersonalAccessToken}"));
                _client = new HttpClient();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            }

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
                var jObject = JObject.Parse(rawEvent);
                var hookModel = jObject.ToObject<BuildEventHook>();
                hookModel.Resource.ProjectId = (string)jObject["resource"]["project"]["id"];
                return await GetBuildModel(hookModel);
            }

            return null;
        }

        private async Task<SlackWorkItemModel> GetWorkItem(WorkItemEventHook hookModel)
        {
            string url = $"{_baseAddress}_apis/wit/workItems/{hookModel.Resource.WorkItemId}";
            var tfsWi = await GetWorkItem(url);
            var slackWorkItemModel = SlackWorkItemModel.FromTfs(tfsWi, hookModel);
            if (tfsWi?.Relations != null && tfsWi.Relations.Any(x => x.Rel == "System.LinkTypes.Hierarchy-Reverse"))
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
            using (var response = await _client.GetAsync($"{workItemUrl}?$expand=relations&api-version=1.0"))
            {
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

        private async Task<SlackBuildModel> GetBuildModel(BuildEventHook hookModel)
        {
            var buildHistory = await GetBuildHistory(hookModel);
            var slackModel = SlackBuildModel.FromEvent(hookModel);
            slackModel.BuildHistoryEmojis = "";
            foreach (var item in buildHistory)
            {
                slackModel.BuildHistoryEmojis += $":tfsbot-build{item}:";
            }

            // find the previous non-canceled build (excluding most recent)
            slackModel.PreviousBuildResult = buildHistory.Reverse<string>().Skip(1).FirstOrDefault(x => x != "canceled");

            return slackModel;
        }

        private async Task<List<string>> GetBuildHistory(BuildEventHook hookModel)
        {
            string url = $"{_baseAddress}{hookModel.Resource.ProjectId}/_apis/build/builds/?api-version=2.0&definitions={hookModel.Resource.Definition.Id}&maxBuildsPerDefinition=5&statusFilter=completed";

            using (var response = await _client.GetAsync(url))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var jObject = JObject.Parse(responseString);
                    var buildHistory = jObject["value"].Select(x => (string)x["result"]).Reverse().ToList();
                    return buildHistory;
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
            string url = $"{_baseAddress}_apis/hooks/subscriptions/?api-version=1.0";
            using (var response = await _client.GetAsync(url))
            {
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
            var postDataString = await GetPostDataForIntegration(integrationGroup);

            using (var content = new StringContent(postDataString, Encoding.UTF8, "application/json"))
            using (var response = await _client.PostAsync($"{_baseAddress}_apis/hooks/subscriptions?api-version=1.0", content))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    Serilog.Log.Information($"Created subscription for {integrationGroup.Name}");
                }
                else
                {
                    Serilog.Log.Warning(
                        $"TFS returned code: {(int)response.StatusCode} {response.StatusCode} while creating subscription for {integrationGroup.Name}");
                    Serilog.Log.Warning(responseString);
                }
            }
        }

        private async void UpdateSubscription(SettingsIntegrationGroupModel integrationGroup, string subscriptionId)
        {
            var postDataString = await GetPostDataForIntegration(integrationGroup);

            using (var content = new StringContent(postDataString, Encoding.UTF8, "application/json"))
            using (var response = await _client.PutAsync($"{_baseAddress}_apis/hooks/subscriptions/{subscriptionId}?api-version=1.0", content))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    Serilog.Log.Information($"Updated subscription for {integrationGroup.Name}");
                }
                else
                {
                    Serilog.Log.Warning(
                        $"TFS returned code: {(int)response.StatusCode} {response.StatusCode} while updating subscription for {integrationGroup.Name}");
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
            using (var response = await _client.DeleteAsync($"{_baseAddress}_apis/hooks/subscriptions/{id}?api-version=1.0"))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    Serilog.Log.Information($"Deleted subscription {id}");
                }
                else
                {
                    Serilog.Log.Warning(
                        $"TFS returned code: {(int)response.StatusCode} {response.StatusCode} while deleting subscription {id}");
                    Serilog.Log.Warning(responseString);
                }
            }
        }

        private async Task<string> GetProjectIdFromName(string projectName)
        {
            string url = $"{_baseAddress}_apis/projects/{projectName}?api-version=1.0";

            using (var response = await _client.GetAsync(url))
            {
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
    }
}