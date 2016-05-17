using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Threading.Tasks;

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
    }
}
