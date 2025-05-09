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

    [DisableRequestSizeLimit] //desabilita el maximo permitido por el servidor de app para que el midleware valide.
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] UploadFileDTO request)
    {
        return Ok(await _fileService.SaveFileAsync(request));
    }

    [HttpGet("download")]
    public async Task<IActionResult> DownloadFile([FromQuery] string filePath)
    {

        byte[] file = await _fileService.GetFileAsBytesAsync(filePath);
        string fileName = Path.GetFileName(filePath);

        return File(file, "application/octet-stream", fileName);
    }

    [HttpGet("download/base64")]
    public async Task<IActionResult> GetFileAsBase64([FromQuery] string filePath)
    {
            string base64 = await _fileService.GetFileAsBase64Async(filePath);
            return Ok(new { FileBase64 = base64 });
        
    }


    [HttpGet("list")]
    public async Task<IActionResult> GetFilesByClave([FromQuery] string key)
    {
        var files = await _fileService.GetFilesByClaveAsync(key);
        return Ok(files);
    }

}
 