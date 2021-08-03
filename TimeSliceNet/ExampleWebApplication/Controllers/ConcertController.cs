using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExampleWebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConcertController : Controller
    {
        private readonly TimeSliceContext _context;

        public ConcertController(TimeSliceContext context)
        {
            _context = context;
        }

        [HttpGet("{location}")]
        public async Task<IActionResult> GetAllConcerts([FromRoute] string location)
        {
            if (string.IsNullOrWhiteSpace(location)) return new BadRequestObjectResult(new StringContent($"The empty location '{location}' is not implemented in this MWE."));

            var result = await _context.Concerts
                .Include(r => r.CommonParent)
                .Include(c => c.TimeSlices)
                .Where(c => c.Location == location.ToLower())
                .ToListAsync();
            return new OkObjectResult(result);
        }
    }
}