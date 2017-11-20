using System;
using System.Text.RegularExpressions;

namespace TfsSlackFactory.Models
{
    public class SlackWorkItemModel : ITfsEvent
    {
        public string EventType { get; set; }
        public string DisplayName { get; set; }
        public string ProjectName { get; set; }
        public string WiUrl { get; set; }
        public string WiType { get; set; }
        public int WiId { get; set; }
        public string WiTitle { get; set; }
        public bool IsBoardColumnChanged { get; set; }
        public bool IsStateChanged { get; set; }
        public bool IsAssigmentChanged { get; set; }
        public string AssignedTo { get; set; }
        public string State { get; set; }
        public string UserName { get; set; }
        public string AssignedToUserName { get; set; }
        public string PreviousState { get; set; }

        public string ParentWiUrl { get; set; }
        public string ParentWiType { get; set; }
        public int ParentWiId { get; set; }
        public string ParentWiTitle { get; set; }
        public string BoardColumn { get; set; }
        public string PreviousBoardColumn { get; set; }
        public int Rev { get; set; }

        public static SlackWorkItemModel FromTfs(TfsWorkItemModel tfsModel, WorkItemEventHook hookModel = null)
        {
            if (tfsModel == null)
            {
                return null;
            }

            var model = new SlackWorkItemModel();
            
            model.DisplayName = GetDisplayName(tfsModel.Fields.ChangedBy);
            model.UserName = GetUserName(tfsModel.Fields.ChangedBy);
            model.AssignedTo = GetDisplayName(tfsModel.Fields.AssignedTo);
            model.AssignedToUserName = GetUserName(tfsModel.Fields.AssignedTo);
            model.ProjectName = tfsModel.Fields.TeamProject;
            model.WiUrl = tfsModel.Links.Html.Href;
            model.WiType = tfsModel.Fields.WorkItemType;
            model.WiId = tfsModel.Id;
            model.WiTitle = tfsModel.Fields.Title;
            model.State = tfsModel.Fields.State;
            model.BoardColumn = tfsModel.Fields.BoardColumn;

            if (hookModel != null)
            {
                model.EventType = hookModel.EventType;
                model.IsAssigmentChanged = hookModel.Resource.Fields.AssignedTo?.NewValue != hookModel.Resource.Fields.AssignedTo?.OldValue;
                model.IsStateChanged = hookModel.Resource.Fields.State?.NewValue != hookModel.Resource.Fields.State?.OldValue;
                model.PreviousState = hookModel.Resource.Fields.State?.OldValue;

                model.IsBoardColumnChanged = hookModel.Resource.Fields.BoardColumn?.NewValue != hookModel.Resource.Fields.BoardColumn?.OldValue;
                model.PreviousBoardColumn = hookModel.Resource.Fields.BoardColumn?.OldValue;
                model.Rev = hookModel.Resource.Rev;
            }

            return model;
        }

        private static string GetUserName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            Regex regex = new Regex(@"\<(.+?)\>");
            var match = regex.Match(input);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return String.Empty;
        }

        private static string GetDisplayName(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || !input.Contains("<"))
            {
                return input;
            }
            return input.Substring(0, input.IndexOf("<", StringComparison.Ordinal) - 1);
        }
    }
}
