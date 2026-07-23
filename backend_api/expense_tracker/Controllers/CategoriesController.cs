using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using expense_tracker.Models;
using expense_tracker.Services;

namespace expense_tracker.Controllers
{
    [Authorize]
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController(CategoryManagementService service) : ControllerBase
    {
        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // ── Categories ──────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await service.GetCategoriesAsync(GetUserId());
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest req)
        {
            try
            {
                var created = await service.CreateAsync(GetUserId(), req);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest req)
        {
            try
            {
                await service.UpdateAsync(GetUserId(), id, req);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await service.DeleteAsync(GetUserId(), id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── Rules ───────────────────────────────────────────────────

        [HttpGet("rules")]
        public async Task<IActionResult> GetRules()
        {
            var rules = await service.GetRulesAsync(GetUserId());
            return Ok(rules);
        }

        [HttpPost("rules")]
        public async Task<IActionResult> CreateRule([FromBody] UpsertRuleRequest req)
        {
            try
            {
                var created = await service.CreateRuleAsync(GetUserId(), req);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("rules/{id:int}")]
        public async Task<IActionResult> UpdateRule(int id, [FromBody] UpsertRuleRequest req)
        {
            try
            {
                await service.UpdateRuleAsync(GetUserId(), id, req);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("rules/{id:int}")]
        public async Task<IActionResult> DeleteRule(int id)
        {
            try
            {
                await service.DeleteRuleAsync(GetUserId(), id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── Suggestions ─────────────────────────────────────────────

        [HttpGet("rules/suggestions")]
        public async Task<IActionResult> GetRuleSuggestions()
        {
            var suggestions = await service.GetRuleSuggestionsAsync(GetUserId());
            return Ok(suggestions);
        }
    }
}
