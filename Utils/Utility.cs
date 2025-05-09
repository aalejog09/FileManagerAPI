using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FileManagerAPI.Utils
{
    public class Utility
    {


        public string RemoveDiacritics(string input)
        {
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public string NormalizeFileName(string originalFileName)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
                return string.Empty;

            string nameOnly = Path.GetFileNameWithoutExtension(originalFileName);
            string? extension = Path.GetExtension(originalFileName);

            // Espacios a guion bajo
            string cleaned = nameOnly.Trim().Replace(" ", "_");

            // Quitar acentos y caracteres no ASCII
            cleaned = RemoveDiacritics(cleaned);
            cleaned = Regex.Replace(cleaned, @"[^a-zA-Z0-9_-]", "");

            // Mayúsculas
            cleaned = cleaned.ToUpperInvariant();

            // Máximo 20 caracteres
            if (cleaned.Length > 20)
                cleaned = cleaned.Substring(0, 20);

            return string.IsNullOrEmpty(extension) ? cleaned : $"{cleaned}{extension}";
        }

        public string NormalizeClave(string originalClave)
        {
            if (string.IsNullOrWhiteSpace(originalClave))
                return string.Empty;


            // Espacios a guion bajo
            string cleaned = originalClave.Trim().Replace(" ", "_");

            // Quitar acentos y caracteres no ASCII
            cleaned = RemoveDiacritics(cleaned);
            cleaned = Regex.Replace(cleaned, @"[^a-zA-Z0-9]", "");

            // Mayúsculas
            cleaned = cleaned.ToUpperInvariant();

            // Máximo 20 caracteres
            if (cleaned.Length > 20)
                cleaned = cleaned.Substring(0, 20);

            return string.IsNullOrEmpty(cleaned) ? cleaned : $"{cleaned}";
        }

        public string NormalizePath(string originalPath)
        {
            if (string.IsNullOrWhiteSpace(originalPath))
                return string.Empty;

            char separator = Path.DirectorySeparatorChar;

            // Uniformar separadores
            string uniformPath = originalPath.Replace('\\', separator).Replace('/', separator);
            var segments = uniformPath.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            var cleanedSegments = new List<string>();

            foreach (var segment in segments)
            {
                string nameOnly = Path.GetFileNameWithoutExtension(segment);
                string? extension = Path.GetExtension(segment);

                string cleaned = nameOnly.Trim().Replace(" ", "_");
                cleaned = RemoveDiacritics(cleaned);
                cleaned = Regex.Replace(cleaned, @"[^a-zA-Z0-9_-]", "");
                cleaned = cleaned.ToUpperInvariant();

                if (cleaned.Length > 20)
                    cleaned = cleaned.Substring(0, 20);

                if (!string.IsNullOrEmpty(extension))
                    cleaned += extension;

                cleanedSegments.Add(cleaned);
            }

            return string.Join(separator, cleanedSegments);
        }

    }
}
