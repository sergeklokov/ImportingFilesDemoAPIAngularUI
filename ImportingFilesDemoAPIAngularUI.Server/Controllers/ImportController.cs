using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ImportingFilesDemoAPIAngularUI.Server.Services;

namespace ImportingFilesDemoAPIAngularUI.Server.Controllers
{
    [ApiController]
    [Route("api/import")]
    public class ImportController : ControllerBase
    {
        private readonly IImportService _importService;

        public ImportController(IImportService importService)
        {
            _importService = importService;
        }

        /// <summary>
        /// Imports a file line-by-line into the Phones database.
        /// Each line becomes one row in dbo.FileImports table.
        /// </summary>
        /// <remarks>
        /// Parameters:
        /// - file: The uploaded file (multipart/form-data). 
        ///   Example: When posting from C:\Repos\...\Parking_Violations_Issued_-_Fiscal_Year_2026_20260918 small.csv,
        ///   only the filename "Parking_Violations_Issued_-_Fiscal_Year_2026_20260918 small.csv" is sent to the server.
        ///   The full disk path is not transmitted; only file.FileName (the filename part) is available on the server.
        /// 
        /// - sourceType: String enum value indicating the file source type. Examples: "Parking", "Driving", "Traffic".
        ///   This value is stored in the SourceType column for all rows imported from this file.
        ///   If empty/null, the column will store NULL.
        /// 
        /// - createdBy: Username or identifier of who uploaded the file (defaults to "system").
        ///   Stored in the CreatedBy column for audit purposes.
        /// 
        /// Database: Creates dbo.FileImports table if it doesn't exist.
        /// Columns: Id (PK), SourceType, LineNumber, RawLine, FileName, CreatedBy, CreatedAt (UTC).
        /// </remarks>
        /// <example>
        /// curl -k -X POST "https://localhost:7161/api/import/file" \
        ///   -F "file=@Parking_Violations_Issued_-_Fiscal_Year_2026_20260918 small.csv" \
        ///   -F "sourceType=Parking" \
        ///   -F "createdBy=john.doe"
        /// </example>
        [HttpPost("file")]
        [RequestSizeLimit(5L * 1024 * 1024 * 1024)] // allow uploads up to 5 GB
        public async Task<IActionResult> ImportFile([FromForm] IFormFile? file, 
            [FromForm] string sourceType = "", [FromForm] string createdBy = "system")
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var result = await _importService.ImportFileAsync(file, sourceType, createdBy);

            if (!result.Success)
                return Problem(detail: result.ErrorMessage, title: "Import failed");

            return Ok(new { Imported = result.Imported, File = result.FileName });
        }

        /// <summary>
        /// Imports a file from a local server file path into the Phones database.
        /// WARNING: Development/testing only — this endpoint reads directly from the server's filesystem.
        /// </summary>
        /// <remarks>
        /// Each line in the file becomes one row in dbo.FileImports.
        /// 
        /// Parameters:
        /// - filePath: Full file path on the server (e.g., C:\Data\Parking_Violations_small.csv or /home/user/data/file.csv)
        /// - sourceType: File source type category (e.g., "Parking", "Driving")
        /// - createdBy: Username/identifier for audit (defaults to "system")
        /// </remarks>
        /// <example>
        /// curl -k -X POST "https://localhost:7161/api/import/from-path" \
        ///   -F "filePath=C:\Repos\ImportingFilesDemoAPIAngularUI\Parking_Violations_Issued_-_Fiscal_Year_2026_20260918 small.csv" \
        ///   -F "sourceType=Parking" \
        ///   -F "createdBy=system"
        /// </example>
        [HttpPost("from-path")]
        public async Task<IActionResult> ImportFileFromPath(
            [FromForm] string filePath,
            [FromForm] string sourceType = "",
            [FromForm] string createdBy = "system")
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest("filePath is required.");

            var result = await _importService.ImportFileFromPathAsync(filePath, sourceType, createdBy);

            if (!result.Success)
                return Problem(detail: result.ErrorMessage, title: "Import failed");

            return Ok(new { Imported = result.Imported, File = result.FileName });
        }
    }
}
