using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Ddi.Registry.Data
{
    public class Service
    {
        public Service() 
        { 
            this.ServiceId = Guid.NewGuid();

            this.TimeToLive = 86400;
            this.Priority = 5;
            this.Weight = 5;
            this.Protocol = "tcp";
        }

        [Required]
        public Guid AssignmentId { get; set; }
        [Required]
        public Guid ServiceId { get; set; }

        [Required]
        [Range(0,65535)]
		[Display(Name = "Port", Description = "The TCP or UDP port on which the service is to be found.")]
        public int Port { get; set; }

        [RegularExpression(@"^([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])(\.([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9]))*$",
            ErrorMessage = "The domain must contain letters, numbers, and dots only")]
        [StringLength(255)]
        [Required]
		[Display(Name = "Hostname", Description = "The canonical hostname of the machine providing the service.")]
        public string Hostname { get; set; }

        [Required]
        [RegularExpression(@"([a-zA-Z0-9]{1,61})",
            ErrorMessage = "The service must contain letters or numbers")]
        [StringLength(50)]
		[Display(Name = "Service Name", Description = "The name of the service.")]
        public string ServiceName { get; set; }

        [Required]
        [RegularExpression(@"([a-zA-Z0-9]{1,15})",
                ErrorMessage = "The protocol must contain letters or numbers")]
        [StringLength(10)]
		[Display(Name = "Protocol", Description = "The transport protocol of the desired service; this is usually either TCP or UDP")]
        public string Protocol { get; set; }

        [Required]
		[Display(Name = "Time to Live", Description = "Standard DNS time to live field, in seconds; used to determine the length of time for which results should be cached.")]
        public int TimeToLive { get; set; }

        [Required]
        [Range(0, 65535)]
		[Display(Name = "Priority", Description = "The priority of the target host. Lower values mean more preferred.")]
        public int Priority { get; set; }

        [Required]
        [Range(0, 65535)]
		[Display(Name = "Weight", Description = "A relative weight for records with the same priority.")]
        public int Weight { get; set; }

    }
}
