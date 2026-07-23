using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using expense_tracker.Services;

namespace expense_tracker.Controllers
{

    [Authorize]
    [Route("api/transactions")]
    [ApiController]
    public class QuerysController : ControllerBase
    {
        private readonly Services.TransactionQueryService _service;
        private readonly Services.CategoryClassifierService _classifier;
        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        }

        private readonly Services.RecurringPaymentService _recurring;

        public QuerysController(Services.TransactionQueryService transactionQueryService, CategoryClassifierService classifier, Services.RecurringPaymentService recurring)
        {
            _service = transactionQueryService;
            _classifier = classifier;
            _recurring = recurring;
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllTransactionsAsync()
        {
            var transactions = await _service.GetAllTransactionsAsync(GetUserId());
          
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

        // NEW: Category-based spending summary
        [HttpGet("summary/category")]
        public async Task<IActionResult> GetCategorySummaryAsync()
        {
            var summary = await _service.GetCategorySummaryAsync(GetUserId());
            return Ok(summary);
        }
 
        // NEW: Get all available categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategoriesAsync()
        {
            var categories = await _service.GetCategoriesAsync();
            return Ok(categories);
        }
 
        // NEW: Re-classify existing transactions
        [HttpPost("reclassify")]
        public async Task<IActionResult> ReclassifyAsync([FromQuery] bool force = false)
        {
            var updated = await _classifier.ReclassifyAsync(GetUserId(), force);
            return Ok(new { message = "Reclassification complete", updated });
        }

        [HttpGet("recurring")]
        public async Task<IActionResult> GetRecurringAsync()
        {
            var summary = await _recurring.GetRecurringAsync(GetUserId());
            return Ok(summary);
        }
    }
}
 
