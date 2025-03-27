using FileManagerAPI.Models;
using FileManagerAPI.Models.DTO;
using FileStorageApi.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Drawing;
using System.Runtime.Versioning;

namespace FileManagerAPI.Services
{
    public class SupportedFileService
    {

        private readonly DataContext _context;

        public SupportedFileService(DataContext context)
        {
            _context = context;
        }
        public async Task<List<SupportedFile>> GetSupportedFileAsync()
        {

            var supportedFileList = await _context.SupportedFiles
                .OrderByDescending(f => f.Id)
                .ToListAsync();

            return supportedFileList;
        }

        public async Task<SupportedFileDTO?> GetFileSettingsAsync(string extension)
        {
            var supportedFile = await _context.SupportedFiles
                .Where(f => f.Extension == extension ) 
                .FirstOrDefaultAsync();
            if (supportedFile == null)
                return null;

            return new SupportedFileDTO { Extension = supportedFile.Extension, MaxSizeKB = supportedFile.MaxSizeKB, Status = supportedFile.Status };
        }

        public async Task<SupportedFileDTO> SaveSupportedFileAsync(SupportedFileDTO supportedFile)
        {
            if (string.IsNullOrWhiteSpace(supportedFile.Extension))
                throw new ArgumentException("The file extension cannot be null or empty.");

            if (supportedFile.MaxSizeKB <= 0)
                throw new ArgumentException("The maximum file size must be greater than zero.");

            // ✅ Convertir la extensión a minúsculas para evitar duplicados con mayúsculas
            supportedFile.Extension = supportedFile.Extension.Trim().ToLower();

            // ✅ Verificar si la extensión ya existe
            var existingFile = await _context.SupportedFiles
                .FirstOrDefaultAsync(f => f.Extension == supportedFile.Extension);

            if (existingFile != null)
                throw new InvalidOperationException($"The extension '{supportedFile.Extension}' is already registered.");


            var newSupportedFile = new SupportedFile { Extension = supportedFile.Extension, MaxSizeKB = supportedFile.MaxSizeKB, Status = true};
            _context.SupportedFiles.Add(newSupportedFile);
            await _context.SaveChangesAsync();
            return new SupportedFileDTO {Extension = newSupportedFile.Extension, MaxSizeKB = newSupportedFile.MaxSizeKB, Status = newSupportedFile.Status } ;
        }

        public async Task<SupportedFileDTO?> UpdateSupportedFileAsync(string extension, decimal maxSizeKB)
        {
            var supportedFile = await _context.SupportedFiles
                .FirstOrDefaultAsync(f => f.Extension == extension.ToLower());

            if (supportedFile == null)
                return null;

            if (maxSizeKB <= 0)
                throw new ArgumentException("The maximum file size must be greater than zero.");

            supportedFile.MaxSizeKB = maxSizeKB;

            _context.SupportedFiles.Update(supportedFile);
            await _context.SaveChangesAsync();



            return new SupportedFileDTO {Extension = supportedFile.Extension, MaxSizeKB = supportedFile.MaxSizeKB, Status = supportedFile.Status } ;
        }

        public async Task<SupportedFileDTO?> UpdateFileStatusAsync(string extension, bool newStatus)
        {
            var supportedFile = await _context.SupportedFiles
                .FirstOrDefaultAsync(f => f.Extension == extension.ToLower());

            if (supportedFile == null)
                return null;

            supportedFile.Status = newStatus;

            _context.SupportedFiles.Update(supportedFile);
            await _context.SaveChangesAsync();

            return new SupportedFileDTO {Extension = supportedFile.Extension, MaxSizeKB = supportedFile.MaxSizeKB, Status = supportedFile.Status } ;
        }

        public async Task<List<SupportedFileDTO>?> GetAllSupportedFilesAsync()
        {
            List<SupportedFile> supportedFileList = await GetSupportedFileAsync();

            if (supportedFileList.Count <= 0)
                return null;
           
            List<SupportedFileDTO> supportedFileDTOList = MapSupportedFileToDTOList(supportedFileList);

            return supportedFileDTOList;
        }

        public List<SupportedFileDTO> MapSupportedFileToDTOList(List<SupportedFile> supportedFileList)
        {
            List<SupportedFileDTO> supportedFileDTOList = new List<SupportedFileDTO>();
            foreach (var item in supportedFileList)
            {
                var supportedFileDTO = new SupportedFileDTO { Extension = item.Extension, MaxSizeKB = item.MaxSizeKB, Status = item.Status };
                supportedFileDTOList.Add(supportedFileDTO);
            }
            return supportedFileDTOList;
        }

    }



}
