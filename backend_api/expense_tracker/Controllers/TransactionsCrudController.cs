using System.Security.Claims;
using expense_tracker.Models;
using expense_tracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace expense_tracker.Controllers
{
    // Shares the api/transactions prefix with QuerysController;
    // no route collisions because these are POST/PUT/DELETE.
    [Authorize]
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsCrudController(TransactionCrudService crud) : ControllerBase
    {
        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Description))
                return BadRequest(new { message = "Description is required" });

            var id = await crud.CreateAsync(GetUserId(), request);
            return Ok(new { id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTransactionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Description))
                return BadRequest(new { message = "Description is required" });

            var updated = await crud.UpdateAsync(GetUserId(), id, request);
            return updated ? Ok() : NotFound();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await crud.DeleteAsync(GetUserId(), id);
            return deleted ? Ok() : NotFound();
        }
    }
}
