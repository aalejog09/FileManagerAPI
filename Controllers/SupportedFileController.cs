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
            try 
            {
                var supportedFile = await _SupportedFileService.SaveSupportedFileAsync(request);
                return Ok(new { Message = "SupportedFile added successfully.", Data = supportedFile });
            }
            catch(Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("allowed-extensions")]
        public async Task<IActionResult> GetAllSupportedFilesAsync()
        {

            var activeSupportedFiles = await _SupportedFileService.GetAllSupportedFilesAsync();
            if (activeSupportedFiles == null)
                return NotFound("There are not supported files");

            return Ok(new {Message="OK", Data=activeSupportedFiles});

        }
        [HttpGet("extension/{extension}")]
        public async Task<IActionResult> GetSupportedFileByExtensionAsync([FromRoute] string extension)
        {

            var supportedFiles = await _SupportedFileService.GetFileSettingsAsync(extension);

            if (supportedFiles == null)
                return NotFound(new {Message=$"the extension [.{extension}] does not exist." , Data ="{}"});

            return Ok(new { Message = "OK", Data = supportedFiles });
        }


        [HttpPut("update")]
        public async Task<IActionResult> UpdateSupportedFile([FromQuery] string extension, [FromQuery] decimal maxSizeKB)
        {
            try
            {
                var updatedFile = await _SupportedFileService.UpdateSupportedFileAsync(extension, maxSizeKB);

                if (updatedFile == null)
                    return NotFound(new { Message = $"The file extension [.{extension}] was not found." });

                return Ok(new { Message = $"The file extension [.{extension}] was updated successfully.", Data = updatedFile });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the supported file.", Error = ex.Message });
            }
        }

        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateFileStatus([FromQuery] string extension, [FromQuery] bool status)
        {
            var updatedFile = await _SupportedFileService.UpdateFileStatusAsync(extension, status);

            if (updatedFile == null)
                return NotFound(new { Message = $"The file extension [.{extension}] was not found." });

            return Ok(new { Message = $"The status of the file extension [.{extension} ] was updated successfully.", Data = updatedFile });
        }


    }
}
