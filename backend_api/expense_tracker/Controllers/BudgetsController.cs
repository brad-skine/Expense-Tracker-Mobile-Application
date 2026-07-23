using System.Security.Claims;
using expense_tracker.Models;
using expense_tracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace expense_tracker.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/budgets")]
    public class BudgetsController(BudgetService budgets) : ControllerBase
    {
        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET api/budgets/progress?year=2026&month=7
        [HttpGet("progress")]
        public async Task<IActionResult> GetProgress([FromQuery] int? year, [FromQuery] int? month)
        {
            var now = DateTime.UtcNow;
            var result = await budgets.GetProgressAsync(
                GetUserId(), year ?? now.Year, month ?? now.Month);
            return Ok(result);
        }

        // PUT api/budgets  { categoryId, monthlyLimit }  — 0 deletes the budget
        [HttpPut]
        public async Task<IActionResult> Upsert([FromBody] UpsertBudgetRequest request)
        {
            if (request.MonthlyLimit < 0)
                return BadRequest(new { message = "Limit must be >= 0" });

            if (request.MonthlyLimit == 0)
                await budgets.DeleteAsync(GetUserId(), request.CategoryId);
            else
                await budgets.UpsertAsync(GetUserId(), request);

            return Ok();
        }
    }
}
