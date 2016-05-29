using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace TfsSlackFactory.Models
{
    public class TfsSubscriptionListModel
    {
        public TfsSubscriptionModel[] value { get; set; }
    }

    public class TfsSubscriptionModel
    {
        public string id { get; set; }
        public string publisherId { get; set; }
        public string eventType { get; set; }
        public string consumerId { get; set; }
        public string consumerActionId { get; set; }
        public PublisherInputs publisherInputs { get; set; }
        public ConsumerInputs consumerInputs { get; set; }

        public string integrationName
        {
            get
            {
                var regex = new Regex(@"integration=(.*)");
                return regex.Match(this.consumerInputs.url).Groups[1].Value;
            }
        }

        public class PublisherInputs
        {
            public string projectId { get; set; }
        }
        public class ConsumerInputs
        {
            public string detailedMessagesToSend { get; set; }
            public string httpHeaders { get; set; }
            public string messagesToSend { get; set; }
            public string resourceDetailsToSend { get; set; }
            public string url { get; set; }
        }
    }

    public class TfsWorkItemModel
    {
        public int Id { get; set; }
        public int Rev { get; set; }
        public RevisionFields Fields { get; set; }
        public Releation[] Relations { get; set; }
        [JsonProperty("_links")]
        public TfsLinks Links { get; set; }
        public string Url { get; set; }

        public class TfsLinks
        {
            public Link Self { get; set; }
            public Link WorkItemUpdates { get; set; }
            public Link WorkItemRevisions { get; set; }
            public Link WorkItemHistory { get; set; }
            public Link Html { get; set; }
            public Link WorkItemType { get; set; }
            public Link Fields { get; set; }
        }

        public class Link
        {
            public string Href { get; set; }
        }

        public class RevisionFields
        {
            [JsonProperty("System.AreaPath")]
            public string AreaPath { get; set; }
            [JsonProperty("System.TeamProject")]
            public string TeamProject { get; set; }
            [JsonProperty("System.IterationPath")]
            public string IterationPath { get; set; }
            [JsonProperty("System.WorkItemType")]
            public string WorkItemType { get; set; }
            [JsonProperty("System.State")]
            public string State { get; set; }
            [JsonProperty("System.AssignedTo")]
            public string AssignedTo { get; set; }
            [JsonProperty("System.Reason")]
            public string Reason { get; set; }
            [JsonProperty("System.CreatedDate")]
            public string CreatedDate { get; set; }
            [JsonProperty("System.CreatedBy")]
            public string CreatedBy { get; set; }
            [JsonProperty("System.ChangedDate")]
            public string ChangedDate { get; set; }
            [JsonProperty("System.ChangedBy")]
            public string ChangedBy { get; set; }
            [JsonProperty("System.Title")]
            public string Title { get; set; }
            [JsonProperty("Microsoft.VSTS.Common.Activity")]
            public string Activity { get; set; }
            [JsonProperty("System.History")]
            public string History { get; set; }
        }

        public class Releation
        {
            public string Rel { get; set; }
            public string Url { get; set; }
        }
    }
}