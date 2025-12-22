using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace expense_tracker.Controllers
{
    [ApiController]
    [Route("api/import")]
    
    public class ImportController : ControllerBase
    {
        private readonly Services.CsvImportService _csvImportService;
        public ImportController(Services.CsvImportService csvImportService)
        {
            _csvImportService = csvImportService;
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> ImportTransactions(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Invalid file uploaded");
            }
            using var stream = file.OpenReadStream();
            var row_count = await _csvImportService.ImportTransactionsAsync(stream);
            return Ok(new { inserted = row_count });
        }
    }
}
