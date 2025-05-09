using FileManagerAPI.Models;
using FileManagerAPI.Models.DTO;
using FileManagerAPI.Services;
using FileManagerAPI.Utils;
using FileManagerAPI.Utils.Exceptions;
using FileStorageApi.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.RegularExpressions;

namespace FileStorageApi.Services
{
    public class FileRecordService(DataContext context, SupportedFileService supportedFileService, ErrorService errorService, Utility utilityService)
    {
        private readonly DataContext _context = context;
        private readonly SupportedFileService _supportedFileService = supportedFileService;
        private readonly ErrorService _errorService = errorService;
        private readonly Utility _utilityService = utilityService;

        public async Task<JSendResponse<string>> SaveFileAsync(UploadFileDTO uploadedFileDTO)
        {
            if (uploadedFileDTO.File == null || uploadedFileDTO.File.Length == 0)
                throw _errorService.GetApiException(ErrorCodes.BadRequest, "El archivo no está presente en el request o está vacío.");

            if (uploadedFileDTO.Clave == null || uploadedFileDTO.Clave.Length == 0 || string.IsNullOrEmpty(uploadedFileDTO.FileName))
                throw _errorService.GetApiException(ErrorCodes.BadRequest, "El Valor Clave es requerido.");

            // Validación del nombre
            if (string.IsNullOrEmpty(uploadedFileDTO.FileName))
                throw _errorService.GetApiException(ErrorCodes.BadRequest, "El nombre del archivo no puede ser nulo o vacío.");
           

            uploadedFileDTO.FileName = _utilityService.NormalizeFileName(uploadedFileDTO.FileName!);
            uploadedFileDTO.Path = _utilityService.NormalizePath(uploadedFileDTO.Path!);
            uploadedFileDTO.Clave = _utilityService.NormalizeClave(uploadedFileDTO.Clave!);
            string[] errors = await ValidateUploadedFileAsync(uploadedFileDTO);
            if (errors.Length != 0)
                throw _errorService.GetApiException(ErrorCodes.BadRequest, errors);


            string fileExtension = Path.GetExtension(uploadedFileDTO.File!.FileName).TrimStart('.').ToLower();
            string folderPath = Path.Combine("", uploadedFileDTO.Path!);

            // Crear carpeta si no existe
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = uploadedFileDTO.FileName + "." + fileExtension;
            string filePath = Path.Combine(folderPath, fileName);
            double size = uploadedFileDTO.File.Length / 1024.0;

            // Guardar archivo (sobrescribe si existe)
            try
            {
                using var stream = new FileStream(filePath, FileMode.Create);
                await uploadedFileDTO.File.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                throw _errorService.GetApiException(ErrorCodes.ServerError, $"Error al guardar el archivo en el disco: {ex.Message}");
            }

            // Buscar si ya existe un registro con misma clave + path + nombre
            var existingRecord = await _context.Files.FirstOrDefaultAsync(f =>
                    f.Clave == uploadedFileDTO.Clave &&
                    f.FilePath == folderPath &&
                    f.FileName == fileName
                );

            if (existingRecord != null)
            {
                // Actualiza datos
                existingRecord.FileSize = size;
                existingRecord.UpdatedAt = DateTime.Now;
            }
            else
            {
                // Crea nuevo registro
                var newFileRecord = new FileRecord
                {
                    Clave = uploadedFileDTO.Clave,
                    FilePath = folderPath,
                    FileName = fileName,
                    FileSize = size
                };
                _context.Files.Add(newFileRecord);
            }

            await _context.SaveChangesAsync();

            return new JSendResponse<string>{ Status = ResponseStatus.SUCCESS,Code = 200, Message = "Archivo cargado con éxito.",Data = filePath};
        }

        public async Task<JSendResponse<List<FileRecordDTO>>> GetFilesByClaveAsync(string clave)
        {
            var files = await _context.Files
            .Where(f => f.Clave == clave)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => MapToDTO(f))
            .ToListAsync();

            if (!files.Any())
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"Archivos no encontrados");

            return new JSendResponse<List<FileRecordDTO>> { Status = ResponseStatus.SUCCESS, Code = 200, Message = "Archivos encontrados con éxito.", Data = files };
            
        }


        public async Task<byte[]> GetFileAsBytesAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"No se encontró el archivo en la ruta especificada.");

            return await File.ReadAllBytesAsync(filePath);
        }

        public async Task<string> GetFileAsBase64Async(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"No se encontró el archivo en la ruta especificada.");
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

        public static FileRecordDTO MapToDTO(FileRecord f)
        {
            return new FileRecordDTO
            {
                FileName = f.FileName,
                FilePath = Path.Combine(f.FilePath, f.FileName),
                CreatedAt = f.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = f.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                Size = $"{f.FileSize:F2} KB"
            };

        }


        public async Task<string[]> ValidateUploadedFileAsync(UploadFileDTO uploadedFileDTO)
        {
            List<string> validationErrors = new();

            if (!Regex.IsMatch(uploadedFileDTO.FileName, @"^[a-zA-Z0-9_-]+$"))
            {
                validationErrors.Add("El nombre del archivo contiene caracteres no permitidos. Solo se permiten letras, números, guión medio(-) y guión bajo(_).");
            }

            if (!Regex.IsMatch(uploadedFileDTO.Clave, @"^[a-zA-Z0-9_-]+$"))
            {
                validationErrors.Add("La clave contiene caracteres no permitidos. Solo se permiten letras, números, guión medio (-) y guión bajo (_).");
            }



            // Obtener la extensión
            string fileExtension = Path.GetExtension(uploadedFileDTO.File.FileName).TrimStart('.').ToLower();

            // Obtener configuración
            var settings = await _supportedFileService.GetFileSettingsAsync(fileExtension);

            if (settings == null)
            {
                validationErrors.Add($"El tipo de archivo '.{fileExtension}' no está permitido.");
            }
            else
            {
                if (!settings.Data!.Status)
                    validationErrors.Add($"El tipo de archivo '.{fileExtension}' está deshabilitado.");

                // Validar tamaño
                var maxFileSize = await _supportedFileService.GetMaxFileSizeByExtension(fileExtension);
                decimal fileSizeKB = (decimal)(uploadedFileDTO.File.Length / 1024.0);

                if (fileSizeKB > maxFileSize.Data)
                {
                    validationErrors.Add($"El tamaño del archivo ({fileSizeKB:F2} KB) excede el máximo permitido ({maxFileSize.Data:F2} KB) para la extensión '.{fileExtension}'.");
                }
            }



            return validationErrors.ToArray();
        }

    }



}
