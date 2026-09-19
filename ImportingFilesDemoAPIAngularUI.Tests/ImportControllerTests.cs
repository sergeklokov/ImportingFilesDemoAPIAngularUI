using System.Diagnostics;
using ImportingFilesDemoAPIAngularUI.Server.Controllers;
using ImportingFilesDemoAPIAngularUI.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ImportingFilesDemoAPIAngularUI.Tests;

public class ImportControllerTests
{
    [Fact]
    public async Task ImportFile_WithNoFile_ReturnsBadRequest()
    {
        var service = new FakeImportService();
        var controller = new ImportController(service);

        var result = await controller.ImportFile(null, "Parking", "tester");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No file uploaded.", badRequest.Value);
        Assert.Equal(0, service.ImportCallCount);
    }

    [Fact]
    public async Task ImportFile_WithFile_ReturnsImportedCount()
    {
        var service = new FakeImportService
        {
            Result = new ImportResult
            {
                Success = true,
                Imported = 2,
                FileName = "sample.csv"
            }
        };
        var controller = new ImportController(service);
        await using var content = new MemoryStream("first line\nsecond line"u8.ToArray());
        var file = new FormFile(content, 0, content.Length, "file", "sample.csv");

        var result = await controller.ImportFile(file, "Parking", "tester");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode ?? 200);
        Assert.Equal(1, service.ImportCallCount);
        Assert.Equal("Parking", service.LastSourceType);
        Assert.Equal("tester", service.LastCreatedBy);
    }

    [Fact]
    public async Task ImportFile_PerformanceSmokeTest_CompletesRepeatedControllerCalls()
    {
        var service = new FakeImportService
        {
            Result = new ImportResult { Success = true, Imported = 1, FileName = "sample.csv" }
        };
        var controller = new ImportController(service);
        const int iterations = 5_000;
        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < iterations; i++)
        {
            await using var content = new MemoryStream("line"u8.ToArray());
            var file = new FormFile(content, 0, content.Length, "file", "sample.csv");
            var result = await controller.ImportFile(file, "Parking", "tester");
            Assert.IsType<OkObjectResult>(result);
        }

        stopwatch.Stop();
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5),
            $"{iterations} controller calls took {stopwatch.Elapsed.TotalMilliseconds:N0} ms.");
    }

    private sealed class FakeImportService : IImportService
    {
        public ImportResult Result { get; set; } = new() { Success = true };
        public int ImportCallCount { get; private set; }
        public string? LastSourceType { get; private set; }
        public string? LastCreatedBy { get; private set; }

        public Task<ImportResult> ImportFileAsync(IFormFile file, string sourceType, string createdBy)
        {
            ImportCallCount++;
            LastSourceType = sourceType;
            LastCreatedBy = createdBy;
            return Task.FromResult(Result);
        }

        public Task<ImportResult> ImportFileBulkAsync(IFormFile file, string sourceType, string createdBy)
        {
            return Task.FromResult(Result);
        }

        public Task<ImportResult> ImportFileFromPathAsync(string filePath, string sourceType, string createdBy)
        {
            return Task.FromResult(Result);
        }
    }
}
