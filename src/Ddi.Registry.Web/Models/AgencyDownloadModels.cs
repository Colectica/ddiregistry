using System;
using System.Collections.Generic;

namespace Ddi.Registry.Web.Models
{
    public class AgencyDownloadModel
    {
        public string AgencyId { get; set; }
        public string Label { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime? DateApproved { get; set; }
        public List<AssignmentDownloadModel> Assignments { get; set; } = new List<AssignmentDownloadModel>();
    }

    public class AssignmentDownloadModel
    {
        public string AssignmentId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsDelegated { get; set; }
        public List<HttpResolverDownloadModel> HttpResolvers { get; set; } = new List<HttpResolverDownloadModel>();
        public List<ServiceDownloadModel> Services { get; set; } = new List<ServiceDownloadModel>();
        public List<DelegationDownloadModel> Delegations { get; set; } = new List<DelegationDownloadModel>();
    }

    public class HttpResolverDownloadModel
    {
        public string ResolutionType { get; set; }
        public string UrlTemplate { get; set; }
    }

    public class ServiceDownloadModel
    {
        public string ServiceName { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
        public int TimeToLive { get; set; }
        public int Priority { get; set; }
        public int Weight { get; set; }
    }

    public class DelegationDownloadModel
    {
        public string NameServer { get; set; }
    }
}
