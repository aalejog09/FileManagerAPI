﻿using System.ComponentModel.DataAnnotations;

namespace FileManagerAPI.Models.DTO
{
    public class UploadFileDTO
    {
        public string? FileName { get; set; }
        public string? Clave { get; set; }
        public string? Path { get; set; }
        public IFormFile? File { get; set; }


    }
}
