using Ddi.Registry.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Ddi.Registry.Web.Controllers
{
    [Route("api/httpresolver/")]
    [ApiController]
    public class ResolverController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ResolverController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{agency}")]
        public async Task<ActionResult<Assignment>> Get(string agency)
        {
            if (string.IsNullOrWhiteSpace(agency))
            {
                return Problem("No agency specified", statusCode: StatusCodes.Status404NotFound);
            }
            Assignment assignment = await _context.Assignments.Where(x => x.AssignmentId == agency).FirstOrDefaultAsync();

            if (assignment == null)
            {
                return Problem("No agency assignment found for " + agency, statusCode: StatusCodes.Status404NotFound);
            }

            // load HTTP resolvers
            await _context.Entry(assignment)
                .Collection(x => x.HttpResolvers)
                .LoadAsync();

            return assignment;
        }

    }
}
