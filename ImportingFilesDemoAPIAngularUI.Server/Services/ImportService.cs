using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ImportingFilesDemoAPIAngularUI.Server.Services
{
    public class ImportService : IImportService
    {
        private readonly IConfiguration _config;

        public ImportService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<ImportResult> ImportFileAsync(IFormFile file, string sourceType, string createdBy)
        {
            var result = new ImportResult { FileName = file?.FileName };

            if (file == null || file.Length == 0)
            {
                result.Success = false;
                result.ErrorMessage = "No file uploaded.";
                return result;
            }

            try
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    return await ImportFromReaderAsync(reader, file.FileName, sourceType, createdBy);
                }
            }
            catch (SqlException ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Database error: {ex.Message}";
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Server error: {ex.Message}";
                return result;
            }
        }

        public async Task<ImportResult> ImportFileFromPathAsync(string filePath, string sourceType, string createdBy)
        {
            var result = new ImportResult { FileName = Path.GetFileName(filePath) };

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.Success = false;
                result.ErrorMessage = $"File not found: {filePath}";
                return result;
            }

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(fileStream))
                {
                    return await ImportFromReaderAsync(reader, Path.GetFileName(filePath), sourceType, createdBy);
                }
            }
            catch (SqlException ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Database error: {ex.Message}";
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Server error: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Core import logic: reads from a StreamReader and inserts lines into the database.
        /// </summary>
        private async Task<ImportResult> ImportFromReaderAsync(StreamReader reader, string fileName, string sourceType, string createdBy)
        {
            var result = new ImportResult { FileName = fileName };
            var connectionString = _config.GetConnectionString("Phones") ?? "Server=localhost;Database=Phones;Trusted_Connection=True;";

            try
            {
                await using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync();

                // Create table if it doesn't exist
                await CreateTableIfNotExistsAsync(conn);

                // Prepare insert command
                var insertSql = @"INSERT INTO dbo.FileImports (SourceType, LineNumber, RawLine, FileName, CreatedBy, CreatedAt)
VALUES (@SourceType, @LineNumber, @RawLine, @FileName, @CreatedBy, @CreatedAt);";

                await using var tran = (SqlTransaction)await conn.BeginTransactionAsync();
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
                    string? line;
                    int lineNumber = 0;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineNumber++;
                        insertCmd.Parameters["@SourceType"].Value = string.IsNullOrEmpty(sourceType) ? (object)DBNull.Value : sourceType;
                        insertCmd.Parameters["@LineNumber"].Value = lineNumber;
                        insertCmd.Parameters["@RawLine"].Value = (object)line ?? DBNull.Value;
                        insertCmd.Parameters["@FileName"].Value = fileName ?? (object)DBNull.Value;
                        insertCmd.Parameters["@CreatedBy"].Value = createdBy ?? (object)DBNull.Value;
                        insertCmd.Parameters["@CreatedAt"].Value = DateTime.UtcNow;

                        await insertCmd.ExecuteNonQueryAsync();
                        imported++;
                    }

                    await tran.CommitAsync();

                    result.Success = true;
                    result.Imported = imported;
                    return result;
                }
                catch (Exception ex)
                {
                    await tran.RollbackAsync();
                    throw;
                }
            }
            catch (SqlException ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Database error: {ex.Message}";
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Server error: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Creates dbo.FileImports table if it doesn't exist.
        /// </summary>
        private async Task CreateTableIfNotExistsAsync(SqlConnection conn)
        {
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

            await using var createCmd = new SqlCommand(createTableSql, conn);
            await createCmd.ExecuteNonQueryAsync();
        }
    }
}
