using FileManagerAPI.Models;
using FileManagerAPI.Models.DTO;
using FileManagerAPI.Services;
using FileStorageApi.Context;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace FileStorageApi.Services
{
    public class FileRecordService
    {
        private readonly DataContext _context;
        private readonly SupportedFileService _supportedFileService;

        public FileRecordService(DataContext context, SupportedFileService supportedFileService)
        {
            _context = context;
            _supportedFileService = supportedFileService;
        }

        public async Task<string> SaveFileAsync(UploadFileDTO uploadedFileDTO)
        {
            if (uploadedFileDTO.File == null || uploadedFileDTO.File.Length == 0)
                throw new ArgumentException("No file provided.");


            // Obtener la extensión del archivo sin el punto
            string fileExtension = Path.GetExtension(uploadedFileDTO.File.FileName).TrimStart('.').ToLower();

            // ✅ Llamar a `SupportedFileService` para obtener la configuración
            var settings = await _supportedFileService.GetFileSettingsAsync(fileExtension);

            if (settings == null)
                throw new ArgumentException($"El tipo de archivo '.{fileExtension}' no está permitido.");

            // Obtener el tamaño del archivo en KB
            decimal fileSizeKB = (decimal)(uploadedFileDTO.File.Length / 1024.0);
            decimal maxSizeKB = (settings.MaxSizeKB / 1024);

            if (fileSizeKB > maxSizeKB)
                throw new ArgumentException($"El tamaño del archivo ({fileSizeKB:F2} KB) excede el máximo permitido ({settings.MaxSizeKB} Bytes). para el tipo de extension [.{fileExtension}]");


            string folderPath = Path.Combine("", uploadedFileDTO.Path, uploadedFileDTO.Clave);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = Path.GetFileName(uploadedFileDTO.File.FileName);
            string filePath = Path.Combine(folderPath, fileName);
            double size = uploadedFileDTO.File.Length/1024.0;

            //guardar el archivo
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadedFileDTO.File.CopyToAsync(stream);
            }


            //crear el registro en DB
            var fileRecord = new FileRecord { Clave = uploadedFileDTO.Clave, FilePath = filePath, FileName = fileName, FileSize = size };
            _context.Files.Add(fileRecord);
            await _context.SaveChangesAsync();

            return filePath;
        }

        public async Task<List<FileRecordDTO>> GetFilesByClaveAsync(string clave)
        {
            var files = await _context.Files
                .Where(f => f.Clave == clave)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FileRecordDTO
                {
                    FilePath = f.FilePath,
                    CreatedAt = f.CreatedAt.ToString("yyyy-MM-dd"),
                    Size = Math.Round(f.FileSize,2)
                })
                .ToListAsync();

            return files;
        }


        public async Task<byte[]> GetFileAsBytesAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("File not found.");

            return await File.ReadAllBytesAsync(filePath);
        }

        public async Task<string> GetFileAsBase64Async(string filePath)
        {
            byte[] fileBytes = await GetFileAsBytesAsync(filePath);
            return Convert.ToBase64String(fileBytes);
        }

        public async Task<bool> DeleteFileAsync(int fileId)
        {
            var fileRecord = await _context.Files.FindAsync(fileId);
            if (fileRecord == null)
                return false;

            if (System.IO.File.Exists(fileRecord.FilePath))
                System.IO.File.Delete(fileRecord.FilePath);

            _context.Files.Remove(fileRecord);
            await _context.SaveChangesAsync();
            return true;
        }


    }



}
