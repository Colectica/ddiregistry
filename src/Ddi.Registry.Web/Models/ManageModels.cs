using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ddi.Registry.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ddi.Registry.Web.Models
{
    public class OverviewModel
    {
        public IEnumerable<Agency> Agencies { get; set; }
        public IEnumerable<ApplicationUser> People { get; set; }
    }

    public class SearchModel
    {
        public string Term { get; set; }
    }

    public class ApproveModel
    {
        public IEnumerable<Agency> Agencies { get; set; }
        public Dictionary<string, ApplicationUser> People { get; set; }
    }

    public class AgencyOverviewModel
    {
        public AgencyOverviewModel()
        {
            this.Services = new Dictionary<string, IEnumerable<Service>>();
            this.Delegations = new Dictionary<string, IEnumerable<Delegation>>();
            this.HttpResolvers = new Dictionary<string, IEnumerable<HttpResolver>> ();
        }
        public Agency Agency { get; set; }
        public ApplicationUser AdminContact { get; set; }
        public ApplicationUser TechnicalContact { get; set; }
        public IEnumerable<Assignment> Assignments { get; set; }

        public Dictionary<string,IEnumerable<Service>> Services { get; set; }
        public Dictionary<string, IEnumerable<Delegation>> Delegations { get; set; }

        public Dictionary<string, IEnumerable<HttpResolver>> HttpResolvers { get; set; }
    }

	public class UnknownAgencyModel
	{
		public string AgencyName { get; set; }
	}

    public class AgencyModel
    {
		/// <summary>
		///  
		/// </summary>
		/// <remarks>
		/// The regular expression tests for: 
		///   - a country code of 2 or three letters
		///   - a period
		///   - After matching one alphanumeric character, /if/ there is a hyphen it /must/ 
		///     be followed by one or more alphanumerics. Repeat as needed.
		/// Length doesn't need to be enforced within the regex because the whole string is 
		/// limited to 50 characters.
		/// </remarks>
		[RegularExpression(@"[a-zA-Z]{2,3}[\.][a-zA-Z0-9](-?[a-zA-Z0-9]+)*",
			ErrorMessage="The agency name should be in the form [country code] dot [name]. For example: us.agencyname")]
        [StringLength(50)]
		[Required(ErrorMessage="An agency name is required.")]
		[Display(Name = "Agency Name", Description="The agency name should be in the form [country code] dot [name]. For example: us.agencyname")]
        public string AgencyId { get; set; }

        [Required(ErrorMessage = "An agency label is required.")]
        [Display(Name = "Agency Label", Description = "You can add a label to the agency, such as an organization name")]
        public string Label { get; set; }

        [Display(Name = "Technical Contact", Description="The technical contact for your agency identifier is the individual or organization authorized to manage any technical issues related to your agency identifier.")]
        public string TechnicalContactId { get; set; }

        [EmailAddress]
        [Display(Name = "Technical Contact Email", Description = "The technical contact for your agency identifier is the individual or organization authorized to manage any technical issues related to your agency identifier.")]
        public string TechnicalContactEmail { get; set; }

        [Display(Name = "Administrative Contact", Description = "An administrative contact is the individual authorized to interact with the DDI Registry on behalf of the registrant specified in the WHOIS record. The administrative contact for your domain has the authorization to accept notices and make requests to the DDI Registry regarding the management of your agency identifier and, along with the technical contact for your agency identifier, will receive all renewal and other administrative notices associated with the agency identifier.")]
        public string AdminContactId { get; set; }

        [EmailAddress]
        [Display(Name = "Administrative Contact Email", Description = "An administrative contact is the individual authorized to interact with the DDI Registry on behalf of the registrant specified in the WHOIS record. The administrative contact for your domain has the authorization to accept notices and make requests to the DDI Registry regarding the management of your agency identifier and, along with the technical contact for your agency identifier, will receive all renewal and other administrative notices associated with the agency identifier.")]
        public string AdminContactEmail { get; set; }

        [Display(Name = "Creator", Description = "The user who initially created the agency.")]
        public string CreatorId { get; set; }
    }

    public class AssignmentModel
    {        
        public string AgencyId { get; set; }
        [RegularExpression(@"^([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])(\.([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9]))*$",
            ErrorMessage = "The sub domain must contain letters, numbers, and dots only, and begin with the agency name")]
        [StringLength(255)]
        [Required]
        public string AssignmentId { get; set; }

        public bool Delegated { get; set; }

        public IEnumerable<HttpResolver> HttpResolvers { get; set; }

        public IEnumerable<Service> Services { get; set; }
		public IEnumerable<Delegation> Delegations { get; set; }

		public bool IsDelegated { get; set; }
	}


}