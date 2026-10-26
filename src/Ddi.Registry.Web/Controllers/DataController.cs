using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ddi.Registry.Data;
using Ddi.Registry.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ddi.Registry.Web.Controllers
{
    [AllowAnonymous]
    [Route("data")]
    public class DataController : Controller
    {
        private static readonly JsonSerializerOptions DownloadJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        private readonly ApplicationDbContext _context;

        public DataController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("agencies.json")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> DownloadAgencies(CancellationToken cancellationToken)
        {
            var agencies = await _context.Agencies
                .AsNoTracking()
                .AsSplitQuery()
                .Where(agency => agency.ApprovalState == ApprovalState.Approved)
                .OrderBy(agency => agency.AgencyId)
                .Select(agency => new AgencyDownloadModel
                {
                    AgencyId = agency.AgencyId,
                    Label = agency.Label,
                    DateCreated = agency.DateCreated,
                    LastModified = agency.LastModified,
                    DateApproved = agency.DateApproved,
                    Assignments = agency.Assignments
                        .OrderBy(assignment => assignment.AssignmentId)
                        .Select(assignment => new AssignmentDownloadModel
                        {
                            AssignmentId = assignment.AssignmentId,
                            DateCreated = assignment.DateCreated,
                            LastModified = assignment.LastModified,
                            IsDelegated = assignment.IsDelegated,
                            HttpResolvers = assignment.HttpResolvers
                                .OrderBy(resolver => resolver.ResolutionType)
                                .ThenBy(resolver => resolver.UrlTemplate)
                                .Select(resolver => new HttpResolverDownloadModel
                                {
                                    ResolutionType = resolver.ResolutionType,
                                    UrlTemplate = resolver.UrlTemplate
                                }).ToList(),
                            Services = assignment.Services
                                .OrderBy(service => service.ServiceName)
                                .ThenBy(service => service.Protocol)
                                .ThenBy(service => service.Hostname)
                                .ThenBy(service => service.Port)
                                .ThenBy(service => service.TimeToLive)
                                .ThenBy(service => service.Priority)
                                .ThenBy(service => service.Weight)
                                .Select(service => new ServiceDownloadModel
                                {
                                    ServiceName = service.ServiceName,
                                    Hostname = service.Hostname,
                                    Port = service.Port,
                                    Protocol = service.Protocol,
                                    TimeToLive = service.TimeToLive,
                                    Priority = service.Priority,
                                    Weight = service.Weight
                                }).ToList(),
                            Delegations = assignment.Delegations
                                .Where(delegation => delegation.Assignment.IsDelegated)
                                .OrderBy(delegation => delegation.NameServer)
                                .Select(delegation => new DelegationDownloadModel
                                {
                                    NameServer = delegation.NameServer
                                }).ToList()
                        }).ToList()
                })
                .ToListAsync(cancellationToken);

            return File(JsonSerializer.SerializeToUtf8Bytes(agencies, DownloadJsonOptions),
                "application/json", "agencies.json");
        }
    }
}
