using System;
using System.Linq;

namespace TfsBot.Models
{
    public class SlackBuildModel : ITfsEvent
    {
        public string EventType { get; set; }
        public string BuildDefinition { get; set; }
        public string Buildstatus { get; set; }
        public string BuildUrl { get; set; }
        public string BuildNumber { get; set; }
        public string BuildReason { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public string BuildDuration => $"{(FinishTime - StartTime).Minutes}m{(FinishTime - StartTime).Seconds}s";
        public string DropLocation { get; set; }

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
            model.Buildstatus = hookEvent.Resource.Status;
            model.BuildUrl = hookEvent.Resource.Url;
            model.UserName = hookEvent.Resource.Requests.First().RequestedFor.UniqueName;
            model.DisplayName = hookEvent.Resource.Requests.First().RequestedFor.DisplayName;
            model.StartTime = DateTime.Parse(hookEvent.Resource.StartTime);
            model.FinishTime = DateTime.Parse(hookEvent.Resource.FinishTime);
            model.DropLocation = hookEvent.Resource.DropLocation;

            return model;
        }
    }
}