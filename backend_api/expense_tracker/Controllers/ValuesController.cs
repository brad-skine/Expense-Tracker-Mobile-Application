using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace expense_tracker.Controllers
{

    [Authorize]
    [Route("api/transactions")]
    [ApiController]
    public class QuerysController : ControllerBase
    {
        private readonly Services.TransactionQueryService _service;

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        }

        public QuerysController(Services.TransactionQueryService transactionQueryService)
        {
            _service = transactionQueryService;
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllTransactionsAsync()
        {
            var transactions = await _service.GetAllTansactionsAsync(GetUserId());
          
            return Ok(transactions);
        }

        [HttpGet("summary/monthly")]
        public async Task<IActionResult> GetMonthlySummaryAsync()
        {
            var summary = await _service.GetMonthlySummaryAsync(GetUserId());
            return Ok(summary);
        }

        [HttpGet("summary/yearly")]
        public async Task<IActionResult> GetYearlySummaryAsync()
        {
            var summary = await _service.GetYearlySummaryAsync(GetUserId());
            return Ok(summary);
        }

        [HttpGet("summary/type")]
        public async Task<IActionResult> GetTypeSummaryAsync()
        {
            var summary = await _service.GetTypeSummaryAsync(GetUserId());
            return Ok(summary);
        }

    }
}
