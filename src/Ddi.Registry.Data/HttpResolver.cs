using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace Ddi.Registry.Data
{
    public class HttpResolver
    {

        public static string ServiceNameWeb { get; } = "website";
        public static string ServiceNameDdiItem { get; } = "ddi";
        public static string ServiceNameDdiSet { get; } = "ddiset";

        public HttpResolver()
        {
            this.Id = Guid.NewGuid().ToString();

        }

        [JsonIgnore]
        [Required]
        public string Id { get; set; }

        [Required]
        public string AssignmentId { get; set; }

        [JsonIgnore]
        public Assignment Assignment { get; set; }

        [Required]
        [Url]
        [Display(Name = "Url Template", Description = "The Url pattern template to redirect the request for a service")]
        public string UrlTemplate { get; set; }

        [Required]
        [RegularExpression(@"([a-zA-Z0-9]{1,61})",
            ErrorMessage = "The resolution type must contain only letters or numbers")]
        [StringLength(50)]
        [Display(Name = "Resolution Type", Description = "The type of endpoint.")]
        public string ResolutionType { get; set; }

        /*
        [Required]
        [Display(Name = "Time to Live", Description = "Standard DNS time to live field, in seconds; used to determine the length of time for which results should be cached.")]
        public int TimeToLive { get; set; }
        */


    }
}
