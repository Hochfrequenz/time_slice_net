using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExampleWebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackstageMeetingController : Controller
    {
        private readonly TimeSliceContext _context;

        public BackstageMeetingController(TimeSliceContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBackstageMeetings()
        {
            var result = await _context.BackstageMeetings
                .Include(r => r.CommonParent)
                .Include(c => c.TimeSlices)
                .ToListAsync();
            return new OkObjectResult(result);
        }
    }
}