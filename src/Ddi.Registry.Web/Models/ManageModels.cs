using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ddi.Registry.Data;
using System.ComponentModel.DataAnnotations;

namespace Ddi.Registry.Web.Models
{
    public class OverviewModel
    {
        public IEnumerable<Agency> Agencies { get; set; }
        public IEnumerable<Person> People { get; set; }
    }

    public class SearchModel
    {
        public string Term { get; set; }
    }

    public class ApproveModel
    {
        public IEnumerable<Agency> Agencies { get; set; }
        public Dictionary<Guid, Person> People { get; set; }
    }

    public class AgencyOverviewModel
    {
        public AgencyOverviewModel()
        {
            this.Services = new Dictionary<Guid, IEnumerable<Service>>();
            this.Delegations = new Dictionary<Guid, IEnumerable<Delegation>>();
        }
        public Agency Agency { get; set; }
        public Person AdminContact { get; set; }
        public Person TechnicalContact { get; set; }
        public IEnumerable<Assignment> Assignments { get; set; }

        public Dictionary<Guid,IEnumerable<Service>> Services { get; set; }
        public Dictionary<Guid, IEnumerable<Delegation>> Delegations { get; set; }
    }

	public class UnknownAgencyModel
	{
		public string AgencyName { get; set; }
	}

    public class AgencyModel
    {
        public Guid AgencyId { get; set; }

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
			ErrorMessage="The agency name should be in the form <country code> dot <name>. For example: us.agencyname")]
        [StringLength(50)]
		[Required(ErrorMessage="An agency name is required.")]
		[Display(Name = "Agency Name", Description="The agency name should be in the form [country code] dot [name]. For example: us.agencyname")]
        public string AgencyName { get; set; }

		[Display(Name = "Technical Contact", Description="The technical contact for your agency identifier is the individual or organization authorized to manage any technical issues related to your agency identifier.")]
        public Guid TechnicalContact { get; set; }

		[Display(Name = "Administrative Contact", Description = "An administrative contact is the individual authorized to interact with the DDI Registry on behalf of the registrant specified in the WHOIS record. The administrative contact for your domain has the authorization to accept notices and make requests to the DDI Registry regarding the management of your agency identifier and, along with the technical contact for your agency identifier, will receive all renewal and other administrative notices associated with the agency identifier.")]
        public Guid AdminContact { get; set; }
    }

    public class AssignmentModel
    {        
        public Guid AgencyId { get; set; }
        public Guid AssignmentId { get; set; }
        [RegularExpression(@"^([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])(\.([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9]))*$", 
            ErrorMessage = "The sub domain must contain letters, numbers, and dots only, and begin with the agency name")]
        [StringLength(255)]
		[Required]
        public string Name { get; set; }
        public string AgencyName { get; set; }
        public bool Delegated { get; set; }

		public IEnumerable<Service> Services { get; set; }
		public IEnumerable<Delegation> Delegations { get; set; }

		public bool IsDelegated { get; set; }
	}

}