using System.Security.Claims;
using expense_tracker.Services.Akahu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace expense_tracker.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/bank-connections")]
    public class BankConnectionsController(AkahuClient akahu, AkahuSyncService sync) : ControllerBase
    {
        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET api/bank-connections
        [HttpGet]
        public async Task<IActionResult> List() =>
            Ok(await sync.GetConnectionsAsync(GetUserId()));

        // POST api/bank-connections/sync-accounts
        [HttpPost("sync-accounts")]
        public async Task<IActionResult> SyncAccounts(CancellationToken ct)
        {
            try
            {
                var count = await sync.SyncAccountsAsync(GetUserId(), ct);
                return Ok(new { synced = count });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { ok = false, message = ex.Message });
            }
        }

        // POST api/bank-connections/sync-transactions?since=2026-06-01
        [HttpPost("sync-transactions")]
        public async Task<IActionResult> SyncTransactions([FromQuery] DateTime? since, CancellationToken ct)
        {
            if (since.HasValue && since.Value.Kind == DateTimeKind.Unspecified)
                since = DateTime.SpecifyKind(since.Value, DateTimeKind.Utc);

            try
            {
                return Ok(await sync.SyncTransactionsAsync(GetUserId(), since?.ToUniversalTime(), ct));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { ok = false, message = ex.Message });
            }
        }

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
