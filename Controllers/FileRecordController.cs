using FileManagerAPI.Models.DTO;
using FileStorageApi.Services;
using Microsoft.AspNetCore.Mvc;

[Route("api/files")]
[ApiController]
public class FileRecordController : ControllerBase
{
    private readonly FileRecordService _fileService;

    public FileRecordController(FileRecordService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] UploadFileDTO request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("No file provided.");

        if (string.IsNullOrEmpty(request.Clave) || string.IsNullOrEmpty(request.Path))
            return BadRequest("Clave and Path are required.");

        try
        {
            string filePath = await _fileService.SaveFileAsync(request);
            return Ok(new { Message = "File uploaded successfully.", FilePath = filePath });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("download")]
    public async Task<IActionResult> DownloadFile([FromQuery] string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            return NotFound("File not found.");

        byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        string fileName = Path.GetFileName(filePath);

        return File(fileBytes, "application/octet-stream", fileName);
    }

    [HttpGet("download/base64")]
    public async Task<IActionResult> GetFileAsBase64([FromQuery] string filePath)
    {
        try
        {
            string base64 = await _fileService.GetFileAsBase64Async(filePath);
            return Ok(new { FileBase64 = base64 });
        }
        catch (FileNotFoundException)
        {
            return NotFound("File not found.");
        }
    }


    [HttpGet("list")]
    public async Task<IActionResult> GetFilesByClave([FromQuery] string key)
    {
        var files = await _fileService.GetFilesByClaveAsync(key);

        if (files == null || files.Count == 0)
            return NotFound("No files found for the given key.");

        return Ok(files);
    }

}
