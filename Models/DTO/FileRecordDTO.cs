using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace FileManagerAPI.Models.DTO
{
    public class FileRecordDTO
    {
        public string? FileName { get; set; }

        [Required]
        public string? FilePath { get; set; }

        public string? CreatedAt { get; set; }

        public string? UpdatedAt { get; set; }

        public string? Size { get; set; }
    }
}
