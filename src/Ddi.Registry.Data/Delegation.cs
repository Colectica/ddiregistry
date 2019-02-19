using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ddi.Registry.Data
{
    public class Delegation
    {
        public Delegation()
        {
            this.DelegationId = Guid.NewGuid().ToString();
        }
        
        public string DelegationId { get; set; }

        [Required]
        [RegularExpression(@"^([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])(\.([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9]))*$",
            ErrorMessage = "The domain must contain letters, numbers, and dots only")]
        [StringLength(255)]
        public string NameServer { get; set; }

        public string AssignmentId { get; set; }
        public Assignment Assignment { get; set; }
    }
}
