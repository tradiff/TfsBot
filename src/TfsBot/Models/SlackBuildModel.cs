using System;
using System.Linq;

namespace TfsBot.Models
{
    public class SlackBuildModel : ITfsEvent
    {
        public string EventType { get; set; }
        public string BuildDefinition { get; set; }
        public string BuildStatus { get; set; }
        public string BuildResult { get; set; }
        public string BuildUrl { get; set; }
        public string BuildNumber { get; set; }
        public string BuildReason { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public string BuildDuration => $"{(FinishTime - StartTime).Minutes}m{(FinishTime - StartTime).Seconds}s";
        public string DropLocation { get; set; }
        public string BuildHistoryEmojis { get; set; }
        public string PreviousBuildResult { get; set; }

        public static SlackBuildModel FromEvent(BuildEventHook hookEvent)
        {
            if (hookEvent == null)
            {
                return null;
            }

            var model = new SlackBuildModel();

            model.EventType = hookEvent.EventType;
            model.BuildDefinition = hookEvent.Resource.Definition.Name;
            model.BuildReason = hookEvent.Resource.Reason;
            model.BuildNumber = hookEvent.Resource.BuildNumber;
            model.BuildStatus = hookEvent.Resource.Status;
            model.BuildResult = hookEvent.Resource.Result;
            model.BuildUrl = hookEvent.Resource.Links.Web.Href;
            model.UserName = hookEvent.Resource.RequestedFor.UniqueName;
            model.DisplayName = hookEvent.Resource.RequestedFor.DisplayName;
            model.StartTime = DateTime.Parse(hookEvent.Resource.StartTime);
            model.FinishTime = DateTime.Parse(hookEvent.Resource.FinishTime);
            model.DropLocation = hookEvent.Resource.DropLocation;

            return model;
        }
    }
}