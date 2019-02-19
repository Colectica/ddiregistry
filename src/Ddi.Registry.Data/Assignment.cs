using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ddi.Registry.Data
{
    public class Assignment
    {
        public Assignment() 
        { 
            this.AssignmentId = Guid.NewGuid();
            this.DateCreated = DateTime.UtcNow;
            this.LastModified = DateTime.UtcNow;
        }

        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }

        public Guid AgencyId { get; set; }
        public Guid AssignmentId { get; set; }

        public string Name { get; set; }
        public bool IsDelegated { get; set; }
    }
}
