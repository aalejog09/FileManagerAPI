using Azure.Core;
using FileManagerAPI.Models;
using FileManagerAPI.Models.DTO;
using FileManagerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Reflection.Metadata.Ecma335;

namespace FileManagerAPI.Controllers
{
    [Route("api/supportedFile")]
    [ApiController]
    public class SupportedFileController : Controller
    {
        private readonly SupportedFileService _SupportedFileService;

        public SupportedFileController(SupportedFileService supportedFileService)
        {
            _SupportedFileService = supportedFileService;
        }


        [HttpPost("save")]
        public async Task<IActionResult> saveSupportedFile([FromBody] SupportedFileDTO request)
        {
                var supportedFile = await _SupportedFileService.SaveSupportedFileAsync(request);
                return Ok(supportedFile);
           
        }

        [HttpGet("allowed-extensions")]
        public async Task<IActionResult> GetAllSupportedFilesAsync()
        {

            var activeSupportedFiles = await _SupportedFileService.GetAllSupportedFilesAsync();
            if (activeSupportedFiles == null)
                return NotFound("There are not supported files");

            return Ok(activeSupportedFiles);

        }
        [HttpGet("extension/{extension}")]
        public async Task<IActionResult> GetSupportedFileByExtensionAsync([FromRoute] string extension)
        {

            var supportedFiles = await _SupportedFileService.GetFileSettingsAsync(extension);

            return Ok(supportedFiles);
        }


        [HttpPut("update")]
        public async Task<IActionResult> UpdateSupportedFile([FromQuery] string extension, [FromQuery] string maxSizeKB)
        {
                var updatedFile = await _SupportedFileService.UpdateSupportedFileAsync(extension, maxSizeKB);
                return Ok(updatedFile);
            
        }

        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateFileStatus([FromQuery] string extension, [FromQuery] bool status)
        {
            var updatedFile = await _SupportedFileService.UpdateFileStatusAsync(extension, status);
            return Ok(updatedFile);
        }


    }
}
