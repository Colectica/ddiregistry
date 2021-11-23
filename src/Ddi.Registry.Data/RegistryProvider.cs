using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Ddi.Registry.Data
{
    public static class RegistryProvider
    {
        public static readonly string ExportAction = "export";
        public static readonly string UpdateAction = "update";

        public static async Task<int> RecordUpdate(this ApplicationDbContext _context)
        {
            ExportAction action = await _context.ExportActions.FindAsync(UpdateAction);
            if(action == null)
            {
                action = new ExportAction() { Id = UpdateAction };
                _context.Add(action);
            }
            action.LastModified = DateTimeOffset.UtcNow;
            int result = await _context.SaveChangesAsync();
            return result;
        }

        public static async Task<bool> ManagesAgency(this ApplicationDbContext _context, string userId, string agencyId)
        {
            if (string.IsNullOrWhiteSpace(userId)) { return false; }
            if (string.IsNullOrWhiteSpace(agencyId)) { return false; }

            Agency agency = await _context.GetAgency(agencyId);
            if (agency != null &&
                (agency.CreatorId == userId || agency.TechnicalContactId == userId || agency.AdminContactId == userId))
            {
                return true;
            }
            return false;
        }

        public static async Task<Agency> GetAgency(this ApplicationDbContext _context, string agencyId)
        {
            var result = await _context.Agencies.FindAsync(agencyId);
            return result;
        }

        public static async Task<Assignment> GetAssignment(this ApplicationDbContext _context, string assignmentId)
        {
            var result = await _context.Assignments.FindAsync(assignmentId);
            return result;
        }

        public static async Task<List<Service>> GetServicesForAssignment(this ApplicationDbContext _context, string assignmentId)
        {
            var result = await _context.Services.Where(x => x.AssignmentId == assignmentId).ToListAsync();
            return result;
        }

        public static async Task<List<Delegation>> GetDelegationsForAssignment(this ApplicationDbContext _context, string assignmentId)
        {
            var result = await _context.Delegations.Where(x => x.AssignmentId == assignmentId).ToListAsync();
            return result;
        }

        public static async Task<List<HttpResolver>> GetHttpResolversForAssignment(this ApplicationDbContext _context, string assignmentId)
        {
            var result = await _context.HttpResolvers.Where(x => x.AssignmentId == assignmentId).ToListAsync();
            return result;
        }


        public static async Task<bool> ManagesService(this ApplicationDbContext _context, string userId, string serviceId)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if(service == null) { return false; }

            var assignment = await _context.Assignments.FindAsync(service.AssignmentId);
            if(assignment == null) { return false; }

            return await _context.ManagesAgency(userId, assignment.AgencyId);
        }

        public static async Task<bool> ManagesHttpResolver(this ApplicationDbContext _context, string userId, string resolverId)
        {
            var resolver = await _context.HttpResolvers.FindAsync(resolverId);
            if (resolver == null) { return false; }

            var assignment = await _context.Assignments.FindAsync(resolver.AssignmentId);
            if (assignment == null) { return false; }

            return await _context.ManagesAgency(userId, assignment.AgencyId);
        }

        public static async Task<List<Agency>> GetAgenciesForUser(this ApplicationDbContext _context, string userId)
        {
            var result = await _context.Agencies.Where(x => x.AdminContactId == userId || x.TechnicalContactId == userId || x.CreatorId == userId).ToListAsync();
            return result;
        }
        public static async Task<List<ApplicationUser>> GetPeopleForUser(this ApplicationDbContext _context, string userId)
        {
            var agencies = await _context.Agencies.Where(x => x.AdminContactId == userId || x.TechnicalContactId == userId || x.CreatorId == userId).ToListAsync();
            var ids = new List<string>();

            ids.Add(userId);

            foreach(var agency in agencies)
            {
                if (!string.IsNullOrWhiteSpace(agency.AdminContactId))
                {
                    ids.Add(agency.AdminContactId);
                }
                if (!string.IsNullOrWhiteSpace(agency.TechnicalContactId))
                {
                    ids.Add(agency.TechnicalContactId);
                }
                if (!string.IsNullOrWhiteSpace(agency.CreatorId))
                {
                    ids.Add(agency.CreatorId);
                }

            }
            var result = await _context.Users.Where(x => ids.Contains(x.Id)).ToListAsync();
            return result;
        }

    }
}
