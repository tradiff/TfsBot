using System;
using System.Text.RegularExpressions;

namespace TfsSlackFactory.Models
{
    public class SlackWorkItemModel
    {
        public string TeamProjectCollection { get; set; }
        public string DisplayName { get; set; }
        public string ProjectName { get; set; }
        public string WiUrl { get; set; }
        public string WiType { get; set; }
        public string WiId { get; set; }
        public string WiTitle { get; set; }
        public string IsStateChanged { get; set; }
        public string IsAssigmentChanged { get; set; }
        public string AssignedTo { get; set; }
        public string State { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string AssignedToUserName { get; set; }
        public string MappedAssignedToUser { get; set; }
        public string MappedUser { get; set; }

        public string ParentWiUrl { get; set; }
        public string ParentWiType { get; set; }
        public string ParentWiId { get; set; }
        public string ParentWiTitle { get; set; }

        public static SlackWorkItemModel FromTfs(TfsWorkItemModel tfsModel)
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
            model.WiUrl = tfsModel.Url;
            model.WiType = tfsModel.Fields.WorkItemType;
            model.WiId = tfsModel.Id.ToString();
            model.WiTitle = tfsModel.Fields.Title;
            model.State = tfsModel.Fields.State;

            return model;
        }

        private static string GetUserName(string input)
        {
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
            if (!input.Contains("<"))
            {
                return input;
            }
            return input.Substring(0, input.IndexOf("<", StringComparison.Ordinal) - 1);
        }
    }
}
