using Azure;
using FileManagerAPI.Models;
using FileManagerAPI.Models.DTO;
using FileManagerAPI.Utils.Exceptions;
using FileStorageApi.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Drawing;
using System.Globalization;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace FileManagerAPI.Services
{
    public class SupportedFileService(DataContext context, ErrorService errorService)
    {

        private readonly DataContext _context = context;
        private readonly ErrorService _errorService = errorService;

        public async Task<JSendResponse<List<SupportedFile>>> GetSupportedFileAsync()
        {

            var supportedFileList = await _context.SupportedFiles
                .OrderByDescending(f => f.Id)
                .ToListAsync();

            if (!supportedFileList.Any())
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"No se encontraron Extensiones");


            return new JSendResponse<List<SupportedFile>> { Status = ResponseStatus.SUCCESS, Code = 200, Message = "Extensiones encontradas.", Data = supportedFileList };

        }

        public async Task<JSendResponse<SupportedFileRsDTO>> GetFileSettingsAsync(string extension)
        {
            var supportedFile = await _context.SupportedFiles
                .Where(f => f.Extension == extension)
                .FirstOrDefaultAsync();

            if (supportedFile == null)
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"No se encontró la extensión: [{extension}].");

            return new JSendResponse<SupportedFileRsDTO> { Status = ResponseStatus.SUCCESS, Code = 200, Message = "Extensión encontrada.", Data = MapToDTO(supportedFile) };

        }

        public async Task<JSendResponse<decimal>> GetMaxFileSizeByExtension(string extension)
        {
            var supportedFile = await _context.SupportedFiles
                .Where(f => f.Extension == extension)
                .FirstOrDefaultAsync();

            if (supportedFile == null)
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"No se encontró la extension: [{extension}].");

            return new JSendResponse<decimal> { Status = ResponseStatus.SUCCESS, Code = 200, Message = "Extensión encontrada.", Data = supportedFile.MaxSizeKB };

        }

        public async Task<JSendResponse<SupportedFileRsDTO>> SaveSupportedFileAsync(SupportedFileDTO supportedFile)
        {
            if (string.IsNullOrWhiteSpace(supportedFile.Extension))
                throw _errorService.GetApiException(ErrorCodes.BadRequest, "La extensión es requerida.");


            string[] errors = ValidateSupportedFile(supportedFile);
            if (errors.Length != 0)
                throw _errorService.GetApiException(ErrorCodes.BadRequest, errors);

            // Normalizamos MaxSizeKB (Reemplazar ',' por '.')
            string normalizedMaxSizeKB = supportedFile.MaxSizeKB.Replace(",", ".");

            // Intentamos convertir el string a decimal
            if (!decimal.TryParse(normalizedMaxSizeKB, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedMaxSizeKB))
            {
                throw _errorService.GetApiException(ErrorCodes.BadRequest, "El formato del tamaño de la extensión es inválido.");
            }

            decimal MaxSizeKbDecimal = parsedMaxSizeKB;

            //  Convertir la extensión a minúsculas para evitar duplicados con mayúsculas
            supportedFile.Extension = supportedFile.Extension.Trim().ToLower();

            // Verificar si la extensión ya existe
            var existingFile = await _context.SupportedFiles
                .FirstOrDefaultAsync(f => f.Extension == supportedFile.Extension);

            if (existingFile != null)
                throw _errorService.GetApiException(ErrorCodes.BadRequest, $"La extensión [{supportedFile.Extension}] ya se encuentra registrada.");


            supportedFile.MaxSizeKB = supportedFile.MaxSizeKB;
            var newSupportedFile = new SupportedFile { Extension = supportedFile.Extension, MaxSizeKB = (MaxSizeKbDecimal), Status = true };
            _context.SupportedFiles.Add(newSupportedFile);
            await _context.SaveChangesAsync();
            return new JSendResponse<SupportedFileRsDTO> { Status = ResponseStatus.SUCCESS, Code = 201, Message = "Extensión registrada con éxito.", Data = MapToDTO(newSupportedFile) };

        }

        public async Task<JSendResponse<SupportedFileRsDTO>> UpdateSupportedFileAsync(string extension, string maxSizeKB)
        {
            if (string.IsNullOrWhiteSpace(extension))
                throw _errorService.GetApiException(ErrorCodes.BadRequest, "La extensión es requerida.");

            string[] errors = ValidateSupportedFile(new SupportedFileDTO { Extension=extension, MaxSizeKB=maxSizeKB});
            if (errors.Length != 0)
                throw _errorService.GetApiException(ErrorCodes.BadRequest, errors);

            var supportedFile = await _context.SupportedFiles
                .FirstOrDefaultAsync(f => f.Extension == extension.ToLower()) ?? throw _errorService.GetApiException(ErrorCodes.NotFound, $"No se encotro la extensión solicitada.");

            if (!decimal.TryParse(maxSizeKB.Trim().Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal sizeKB))
            {
                throw _errorService.GetApiException(ErrorCodes.BadRequest, "El campo MaxSizeKB debe ser un número decimal válido.");
            }

            supportedFile.MaxSizeKB = sizeKB;

            _context.SupportedFiles.Update(supportedFile);
            await _context.SaveChangesAsync();

            return new JSendResponse<SupportedFileRsDTO> { Status = ResponseStatus.SUCCESS, Code = 200, Message = "Extensión actualizada con éxito.", Data = MapToDTO(supportedFile) };

        }

        public async Task<JSendResponse<SupportedFileRsDTO>> UpdateFileStatusAsync(string extension, bool newStatus)
        {
            var supportedFile = await _context.SupportedFiles
                .FirstOrDefaultAsync(f => f.Extension == extension.ToLower()) ?? throw _errorService.GetApiException(ErrorCodes.NotFound, $"No se encotro la extensión solicitada.");
            supportedFile.Status = newStatus;

            _context.SupportedFiles.Update(supportedFile);
            await _context.SaveChangesAsync();
            return new JSendResponse<SupportedFileRsDTO> { Status = ResponseStatus.SUCCESS, Code = 200, Message = "Extensión actualizada con éxito.", Data = MapToDTO(supportedFile) };

        }

        public async Task<JSendResponse<List<SupportedFileRsDTO>>> GetAllSupportedFilesAsync()
        {

            var response = await GetSupportedFileAsync();

            if (response.Data == null || !response.Data.Any())
                throw _errorService.GetApiException(ErrorCodes.NotFound, $"No se encotraron extensiones.");


            return new JSendResponse<List<SupportedFileRsDTO>> { Status = ResponseStatus.SUCCESS, Code = 200, Message = "Extensiones encontradas", Data = MapSupportedFileToDTOList(response.Data) };

        }

        public List<SupportedFileRsDTO> MapSupportedFileToDTOList(List<SupportedFile> supportedFileList)
        {
            List<SupportedFileRsDTO> supportedFileDTOList = new List<SupportedFileRsDTO>();
            foreach (var item in supportedFileList)
            {
                var supportedFileDTO = MapToDTO(item);
                supportedFileDTOList.Add(supportedFileDTO);
            }
            return supportedFileDTOList;
        }




        public SupportedFileRsDTO MapToDTO(SupportedFile entity)
        {
            return new SupportedFileRsDTO { Extension = entity.Extension, MaxSizeKB = $"{entity.MaxSizeKB:F2} KB", Status = entity.Status };
        }



        public string[] ValidateSupportedFile(SupportedFileDTO input)
        {
            List<string> validationErrors =
            [
                .. ValidateExtension(input.Extension),
                .. ValidateMaxSizeKB(input.MaxSizeKB),
            ];

            return validationErrors.ToArray();
        }

        private static List<string> ValidateExtension(string? extension)
        {
            List<string> errors = new();

            if (string.IsNullOrWhiteSpace(extension))
            {
                errors.Add("La extensión no puede estar vacía.");
            }
            else
            {
                if (extension.Length > 5)
                    errors.Add("La extensión no puede tener más de 5 caracteres.");

                if (!Regex.IsMatch(extension, @"^[a-zA-Z1-9]+$"))
                    errors.Add("La extensión solo puede contener letras (a-z, A-Z) y números (1-9), sin acentos ni caracteres especiales.");
            }

            return errors;
        }

        private List<string> ValidateMaxSizeKB(string? maxSizeKB)
        {
            List<string> errors = new();

            const decimal maxAllowed = 29296.87m; // 30 MB en KB
            string? normalizedSize = maxSizeKB?.Trim();

            if (!decimal.TryParse(normalizedSize, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal sizeKB))
            {
                errors.Add("El campo MaxSizeKB debe ser un número válido.");
            }
            else
            {
                if (sizeKB > maxAllowed)
                    errors.Add($"El tamaño máximo permitido es {maxAllowed:F2} KB.");
            }

            return errors;
        }


    }



}
