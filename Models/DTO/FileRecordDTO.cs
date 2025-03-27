using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace FileManagerAPI.Models.DTO
{
    public class FileRecordDTO
    {

        [Required]
        public string FilePath { get; set; }

        public string CreatedAt { get; set; } 

        public double Size { get; set; }
    }
}
