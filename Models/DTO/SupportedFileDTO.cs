using System.ComponentModel.DataAnnotations;

namespace FileManagerAPI.Models.DTO
{
    public class SupportedFileDTO
    {
       
        public required string Extension { get; set; }

        [Required]
        public decimal MaxSizeKB { get; set; }

        public bool Status { get; set; } = true;
    }
}