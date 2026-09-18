using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.IO;

namespace ImportingFilesDemoAPIAngularUI.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ImportController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("file")]
        [RequestSizeLimit(100_000_000)] // allow larger uploads (100 MB)
        public async Task<IActionResult> ImportFile([FromForm] IFormFile file, [FromForm] string sourceType = "", [FromForm] string createdBy = "system")
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var connectionString = _config.GetConnectionString("Phones") ?? "Server=localhost;Database=Phones;Trusted_Connection=True;";

            try
            {
                await using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync();

                // create table if not exists
                var createTableSql = @"IF OBJECT_ID('dbo.FileImports','U') IS NULL
BEGIN
    CREATE TABLE dbo.FileImports (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SourceType NVARCHAR(200) NULL,
        LineNumber INT NULL,
        RawLine NVARCHAR(MAX) NULL,
        FileName NVARCHAR(260) NULL,
        CreatedBy NVARCHAR(200) NULL,
        CreatedAt DATETIME2 NULL
    );
END";

                await using (var createCmd = new SqlCommand(createTableSql, conn))
                {
                    await createCmd.ExecuteNonQueryAsync();
                }

                // prepare insert command
                var insertSql = @"INSERT INTO dbo.FileImports (SourceType, LineNumber, RawLine, FileName, CreatedBy, CreatedAt)
VALUES (@SourceType, @LineNumber, @RawLine, @FileName, @CreatedBy, @CreatedAt);";

                using var tran = conn.BeginTransaction();
                try
                {
                    await using var insertCmd = new SqlCommand(insertSql, conn, tran);
                    insertCmd.Parameters.Add(new SqlParameter("@SourceType", SqlDbType.NVarChar, 200));
                    insertCmd.Parameters.Add(new SqlParameter("@LineNumber", SqlDbType.Int));
                    insertCmd.Parameters.Add(new SqlParameter("@RawLine", SqlDbType.NVarChar, -1));
                    insertCmd.Parameters.Add(new SqlParameter("@FileName", SqlDbType.NVarChar, 260));
                    insertCmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.NVarChar, 200));
                    insertCmd.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime2));

                    int imported = 0;
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        string? line;
                        int lineNumber = 0;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            lineNumber++;
                            insertCmd.Parameters["@SourceType"].Value = string.IsNullOrEmpty(sourceType) ? (object)DBNull.Value : sourceType;
                            insertCmd.Parameters["@LineNumber"].Value = lineNumber;
                            insertCmd.Parameters["@RawLine"].Value = (object)line ?? DBNull.Value;
                            insertCmd.Parameters["@FileName"].Value = file.FileName ?? (object)DBNull.Value;
                            insertCmd.Parameters["@CreatedBy"].Value = createdBy ?? (object)DBNull.Value;
                            insertCmd.Parameters["@CreatedAt"].Value = DateTime.UtcNow;

                            await insertCmd.ExecuteNonQueryAsync();
                            imported++;
                        }
                    }

                    tran.Commit();

                    return Ok(new { Imported = imported, File = file.FileName });
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
            catch (SqlException ex)
            {
                return Problem(detail: ex.Message, title: "Database error");
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Server error");
            }
        }
    }
}
