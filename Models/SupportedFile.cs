using System.ComponentModel.DataAnnotations;

namespace FileManagerAPI.Models
{
    public class SupportedFile
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public required string Extension { get; set; }

        [Required]
        public decimal MaxSizeKB { get; set; }

        public bool Status { get; set; } = true; 
    }
}
