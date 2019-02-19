using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Ddi.Registry.Data
{
    public class Delegation
    {
        public Delegation() { this.DelegationId = Guid.NewGuid(); }

        public Guid AssignmentId { get; set; }
        public Guid DelegationId { get; set; }
        [Required]
        [RegularExpression(@"^([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])(\.([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9]))*$",
            ErrorMessage = "The domain must contain letters, numbers, and dots only")]
        [StringLength(255)]
        public string NameServer { get; set; }
    }
}
