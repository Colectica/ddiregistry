using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Ddi.Registry.Data
{
    public class Person
    {
        public Person() 
        { 
            this.PersonId = Guid.NewGuid();
            this.Name = "";
            this.Email = "";
        }

        public string Username { get; set; }
        
        public Guid PersonId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }

        public string Organization { get; set; }
        public string JobTitle { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string HomePage { get; set; }
    }
}
