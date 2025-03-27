using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace FileManagerAPI.Models
{
    public class FileRecord
    {
        public int Id { get; set; }

        
        [Required]
        
        [MaxLength(255)]
        public string Clave { get; set; } 

        [Required]
        public string FilePath { get; set; }

        public string FileName { get; set; }

        public double FileSize { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
