using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace expense_tracker.Controllers
{
    [Route("api/transactions")]
    [ApiController]
    public class QuerysController : ControllerBase
    {
        private readonly Services.TransactionQueryService _service;

        public QuerysController(Services.TransactionQueryService transactionQueryService)
        {
            _service = transactionQueryService;
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllTransactionsAsync()
        {
            var transactions = await _service.GetAllTansactionsAsync();
          
            return Ok(transactions);
        }

        [HttpGet("summary/monthly")]
        public async Task<IActionResult> GetMonthlySummaryAsync()
        {
            var summary = await _service.GetMonthlySummaryAsync();
            return Ok(summary);
        }

        [HttpGet("summary/yearly")]
        public async Task<IActionResult> GetYearlySummaryAsync()
        {
            var summary = await _service.GetYearlySummaryAsync();
            return Ok(summary);
        }

        [HttpGet("summary/type")]
        public async Task<IActionResult> GetTypeSummaryAsync()
        {
            var summary = await _service.GetTypeSummaryAsync();
            return Ok(summary);
        }

    }
}
