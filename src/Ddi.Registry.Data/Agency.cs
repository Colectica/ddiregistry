using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ddi.Registry.Data
{
    public class Agency
    {
        public Agency()
        {
            //this.AgencyId = Guid.NewGuid().ToString();
            this.DateCreated = DateTime.UtcNow;
            this.LastModified = DateTime.UtcNow;
            this.ApprovalState = Data.ApprovalState.None;
        }

        [Key]
        [Required]
        public string AgencyId { get; set; }

        public string Label { get; set; }

        //[Required]
        //public string AgencyName { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime? DateApproved { get; set; }
        public ApprovalState ApprovalState { get; set; }

        public string CreatorId { get; set; }
        public ApplicationUser Creator { get; set; }

        public string TechnicalContactId { get; set; }
        public ApplicationUser TechnicalContact { get; set; }

        public string AdminContactId { get; set; }
        public ApplicationUser AdminContact { get; set; }

        public List<Assignment> Assignments { get; set; }
    }

    public enum ApprovalState
    {
        None = 0,
        Requested = 10,
        Approved = 20,
        Deprecated = 30
    }
}
