using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ddi.Registry.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Delegation> Delegations { get; set; }
        public DbSet<ExportAction> ExportActions { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<HttpResolver> HttpResolvers { get; set; }
    }
}
