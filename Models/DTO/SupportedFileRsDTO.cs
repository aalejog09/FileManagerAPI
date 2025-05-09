using System.ComponentModel.DataAnnotations;

namespace FileManagerAPI.Models.DTO
{
    public class SupportedFileRsDTO
    {

        public required string Extension { get; set; }
        public required string MaxSizeKB { get; set; }

        public bool Status { get; set; } = true;
    }
}
