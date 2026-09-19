using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ImportingFilesDemoAPIAngularUI.Server.Services
{
    /// <summary>
    /// Service for importing files into the Phones database.
    /// </summary>
    public interface IImportService
    {
        /// <summary>
        /// Imports a file line-by-line. Each line becomes one row in dbo.FileImports.
        /// </summary>
        /// <param name="file">The uploaded file. File.FileName contains only the filename, not the full path.</param>
        /// <param name="sourceType">
        /// Enum value indicating the file source type. Examples: "Parking", "Driving", "Traffic".
        /// This value is stored in the SourceType column for audit/categorization purposes.
        /// </param>
        /// <param name="createdBy">Username or identifier of who uploaded the file (for audit).</param>
        /// <returns>ImportResult containing Success, Imported count, FileName, and ErrorMessage if failed.</returns>
        Task<ImportResult> ImportFileAsync(IFormFile file, string sourceType, string createdBy);

        /// <summary>
        /// Imports a file using SQL Server bulk copy in bounded batches.
        /// </summary>
        Task<ImportResult> ImportFileBulkAsync(IFormFile file, string sourceType, string createdBy);

        /// <summary>
        /// Imports a file from a local file path. Each line becomes one row in dbo.FileImports.
        /// WARNING: This endpoint is for development/local testing only. It reads directly from the server's filesystem.
        /// </summary>
        /// <param name="filePath">Full file path on the server (e.g., C:\Data\file.csv)</param>
        /// <param name="sourceType">Enum value indicating the file source type (e.g., "Parking", "Driving")</param>
        /// <param name="createdBy">Username or identifier of who imported the file (for audit)</param>
        /// <returns>ImportResult containing Success, Imported count, FileName, and ErrorMessage if failed.</returns>
        Task<ImportResult> ImportFileFromPathAsync(string filePath, string sourceType, string createdBy);
    }

    /// <summary>
    /// Result of a file import operation.
    /// </summary>
    public class ImportResult
    {
        public bool Success { get; set; }
        public int Imported { get; set; }
        public string FileName { get; set; }
        public string ErrorMessage { get; set; }
    }
}
