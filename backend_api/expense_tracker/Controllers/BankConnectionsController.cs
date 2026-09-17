using System.Security.Claims;
using expense_tracker.Services.Akahu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace expense_tracker.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/bank-connections")]
    public class BankConnectionsController(AkahuClient akahu) : ControllerBase
    {
        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Smoke test: proves the Akahu tokens work. Remove or lock down later.
        [HttpGet("test")]
        public async Task<IActionResult> Test(CancellationToken ct)
        {
            try
            {
                var me = await akahu.GetMeAsync(ct);
                return Ok(new { ok = true, akahuUser = me.Email ?? me.Id });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { ok = false, message = ex.Message });
            }
        }
    }
}
