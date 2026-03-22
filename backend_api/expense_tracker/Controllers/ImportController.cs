using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;  

namespace expense_tracker.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/import")] 
    
    public class ImportController(Services.CsvImportService csvImportService) : ControllerBase
    {
        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> ImportTransactions(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Invalid file uploaded");
            }
            
            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("needs a csv file");
            }

            var userId = GetUserId();
            await using var stream = file.OpenReadStream();
            var rowCount = await csvImportService.ImportTransactionsAsync(stream, userId);
            return Ok(new { 
                message = "Import successful and data loaded into database",
                inserted = rowCount
            });
        }
    }
}
