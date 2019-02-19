using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Ddi.Registry.Data
{
    public class Agency
    {
        public Agency() 
        { 
            this.AgencyId = Guid.NewGuid();
            this.DateCreated = DateTime.UtcNow;
            this.LastModified = DateTime.UtcNow;
            this.ApprovalState = Data.ApprovalState.None;
        }

        [Required]
        public Guid AgencyId { get; set; }
        public string Username { get; set; }
        
		[Required]
        public string AgencyName { get; set; }
        
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime? DateApproved { get; set; }
        public ApprovalState ApprovalState { get; set; }

        public Guid TechnicalContactId { get; set; }
        public Guid AdminContactId { get; set; }
    }

    public enum ApprovalState
    {
        None = 0,
        Requested = 10,
        Approved = 20,
        Deprecated = 30
    }
}
