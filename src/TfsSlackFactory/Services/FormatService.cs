using TfsSlackFactory.Models;

namespace TfsSlackFactory.Services
{
    public class FormatService
    {
        public string Format(SlackWorkItemModel model, string formatString)
        {
            string message = formatString;
            message = message.Replace("{teamProjectCollection}", model.TeamProjectCollection);
            message = message.Replace("{dDisplayName}", model.DisplayName);
            message = message.Replace("{projectName}", model.ProjectName);
            message = message.Replace("{wiUrl}", model.WiUrl);
            message = message.Replace("{wiType}", model.WiType);
            message = message.Replace("{wiId}", model.WiId);
            message = message.Replace("{wiTitle}", model.WiTitle);
            message = message.Replace("{isStateChanged}", model.IsStateChanged);
            message = message.Replace("{isAssigmentChanged}", model.IsAssigmentChanged);
            message = message.Replace("{assignedTo}", model.AssignedTo);
            message = message.Replace("{state}", model.State);
            message = message.Replace("{userName}", model.UserName);
            message = message.Replace("{action}", model.Action);
            message = message.Replace("{assignedToUserName}", model.AssignedToUserName);
            message = message.Replace("{mappedAssignedToUser}", model.MappedAssignedToUser);
            message = message.Replace("{mappedUser}", model.MappedUser);
            message = message.Replace("{parentWiUrl}", model.ParentWiUrl);
            message = message.Replace("{parentWiType}", model.ParentWiType);
            message = message.Replace("{parentWiId}", model.ParentWiId);
            message = message.Replace("{parentWiTitle}", model.ParentWiTitle);
            return message;
        }
    }
}