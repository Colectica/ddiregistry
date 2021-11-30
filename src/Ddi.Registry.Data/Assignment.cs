using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Ddi.Registry.Data
{
    public class Assignment
    {
        public Assignment()
        {
            //this.AssignmentId = Guid.NewGuid().ToString();
            this.DateCreated = DateTime.UtcNow;
            this.LastModified = DateTime.UtcNow;
        }

        [Key]
        [Required]
        public string AssignmentId { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }

        public string AgencyId { get; set; }
        [JsonIgnore]
        public Agency Agency { get; set; }

        //public string Name { get; set; }
        [JsonIgnore]
        public bool IsDelegated { get; set; }

        [JsonIgnore]
        public List<Delegation> Delegations { get; set; }
        [JsonIgnore]
        public List<Service> Services { get; set; }

        public List<HttpResolver> HttpResolvers { get; set; }
    }
}
