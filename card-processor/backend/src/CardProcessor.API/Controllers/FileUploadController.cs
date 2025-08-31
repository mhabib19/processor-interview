using CardProcessor.API.DTOs;
using CardProcessor.Application.Services;
using CardProcessor.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CardProcessor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileUploadController : ControllerBase
{
    private readonly ITransactionProcessor _transactionProcessor;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IConfiguration _configuration;
    private static readonly Dictionary<string, ProcessingStatusResponse> _processingStatus = new();
    private static readonly Dictionary<string, string> _filePaths = new(); // Store file paths
    private static readonly Dictionary<string, bool> _realDataFlags = new(); // Store real data flags

    public FileUploadController(ITransactionProcessor transactionProcessor, ITransactionRepository transactionRepository, IConfiguration configuration)
    {
        _transactionProcessor = transactionProcessor;
        _transactionRepository = transactionRepository;
        _configuration = configuration;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<FileUploadResponse>> UploadFile(IFormFile file, [FromForm] bool isRealData = false)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new FileUploadResponse
                {
                    Success = false,
                    ErrorMessage = "No file provided"
                });
            }

            var maxFileSizeMB = _configuration.GetValue<int>("MaxFileSizeMB", 10);
            if (file.Length > maxFileSizeMB * 1024 * 1024)
            {
                return BadRequest(new FileUploadResponse
                {
                    Success = false,
                    ErrorMessage = $"File size exceeds {maxFileSizeMB}MB limit"
                });
            }

            var allowedExtensions = new[] { ".csv", ".json", ".xml" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new FileUploadResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid file type. Only CSV, JSON, and XML files are allowed"
                });
            }

            // Generate a unique file ID
            var fileId = Guid.NewGuid().ToString();
            
            // Store file temporarily with correct extension (in production, you'd save to cloud storage)
            var tempDirectory = Path.GetTempPath();
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var tempPath = Path.Combine(tempDirectory, fileName);
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Store the file path and real data flag for processing
            _filePaths[fileId] = tempPath;
            _realDataFlags[fileId] = isRealData;

            // Initialize processing status
            _processingStatus[fileId] = new ProcessingStatusResponse
            {
                Success = true,
                Status = "pending",
                TotalRecords = 0,
                ValidRecords = 0,
                RejectedRecords = 0,
                ProcessingTime = null,
                ErrorMessage = null,
                StartedAt = DateTime.UtcNow
            };

            var response = new FileUploadResponse
            {
                Success = true,
                FileId = fileId,
                FileName = file.FileName,
                FileSize = file.Length,
                ErrorMessage = null
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new FileUploadResponse
            {
                Success = false,
                ErrorMessage = $"Upload failed: {ex.Message}"
            });
        }
    }

    [HttpPost("process/{fileId}")]
    public ActionResult<ProcessingStatusResponse> ProcessFile(string fileId)
    {
        try
        {
            if (!_processingStatus.ContainsKey(fileId))
            {
                return NotFound(new ProcessingStatusResponse
                {
                    Success = false,
                    Status = "failed",
                    ErrorMessage = "File not found"
                });
            }

            var status = _processingStatus[fileId];
            if (status.Status == "processing" || status.Status == "completed")
            {
                return Ok(status);
            }

            // Update status to processing
            status.Status = "processing";
            _processingStatus[fileId] = status;

            // Start processing in background (in production, use a proper job queue)
            _ = Task.Run(async () => await ProcessFileAsync(fileId));

            return Ok(status);
        }
        catch (Exception ex)
        {
            return BadRequest(new ProcessingStatusResponse
            {
                Success = false,
                Status = "failed",
                ErrorMessage = $"Processing failed: {ex.Message}"
            });
        }
    }

    [HttpGet("status/{fileId}")]
    public ActionResult<ProcessingStatusResponse> GetProcessingStatus(string fileId)
    {
        if (!_processingStatus.ContainsKey(fileId))
        {
            return NotFound(new ProcessingStatusResponse
            {
                Success = false,
                Status = "failed",
                ErrorMessage = "File not found"
            });
        }

        return Ok(_processingStatus[fileId]);
    }

    [HttpDelete("{fileId}")]
    public ActionResult DeleteFile(string fileId)
    {
        if (_processingStatus.ContainsKey(fileId))
        {
            _processingStatus.Remove(fileId);
        }

        // Clean up file path and temporary file
        if (_filePaths.ContainsKey(fileId))
        {
            var filePath = _filePaths[fileId];
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to clean up temporary file {filePath}: {ex.Message}");
            }
            _filePaths.Remove(fileId);
            _realDataFlags.Remove(fileId);
        }

        return Ok();
    }

    [HttpGet("max-size")]
    public ActionResult<int> GetMaxFileSize()
    {
        var maxFileSizeMB = _configuration.GetValue<int>("MaxFileSizeMB", 10);
        return Ok(maxFileSizeMB);
    }

    private async Task ProcessFileAsync(string fileId)
    {
        try
        {
            var status = _processingStatus[fileId];
            var startTime = DateTime.UtcNow;

            // Get the actual uploaded file path
            if (!_filePaths.ContainsKey(fileId))
            {
                throw new InvalidOperationException("File path not found for processing");
            }

            var filePath = _filePaths[fileId];
            var isRealData = _realDataFlags[fileId];

            // Process the actual uploaded file using the transaction processor
            var result = await _transactionProcessor.ProcessFileAsync(filePath, isRealData);

            var processingTime = DateTime.UtcNow - startTime;

            // Update status with actual processing results
            status.Success = result.Success;
            status.Status = result.Success ? "completed" : "failed";
            status.TotalRecords = result.TotalRecords;
            status.ValidRecords = result.ValidRecords;
            status.RejectedRecords = result.RejectedRecords;
            status.ProcessingTime = $"{processingTime.TotalSeconds:F2}s";
            status.ErrorMessage = result.ErrorMessage;
            status.CompletedAt = DateTime.UtcNow;

            _processingStatus[fileId] = status;

            // Clean up the temporary file after processing
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                _filePaths.Remove(fileId);
                _realDataFlags.Remove(fileId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to clean up temporary file {filePath}: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            var status = _processingStatus[fileId];
            status.Status = "failed";
            status.ErrorMessage = ex.Message;
            _processingStatus[fileId] = status;
        }
    }
}


