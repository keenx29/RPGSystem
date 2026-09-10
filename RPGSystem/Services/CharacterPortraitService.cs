using Microsoft.AspNetCore.Http;

namespace RPGSystem.Services
{
    public class CharacterPortraitService
    {
        private const long MaxFileSize = 2 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".png",
                ".jpg",
                ".jpeg",
                ".webp"
            };

        private readonly IWebHostEnvironment _environment;

        public CharacterPortraitService(
            IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> SavePortraitAsync(
            IFormFile? portraitFile)
        {
            if (portraitFile == null || portraitFile.Length == 0)
                return null;

            if (portraitFile.Length > MaxFileSize)
            {
                throw new InvalidOperationException(
                    "Portrait image cannot be larger than 2 MB.");
            }

            var extension = Path.GetExtension(portraitFile.FileName);

            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    "Portrait image must be PNG, JPG, JPEG, or WEBP.");
            }

            var portraitsDirectory = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "portraits");

            Directory.CreateDirectory(portraitsDirectory);

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

            var filePath = Path.Combine(
                portraitsDirectory,
                fileName);

            await using var stream = new FileStream(
                filePath,
                FileMode.Create);

            await portraitFile.CopyToAsync(stream);

            return $"/uploads/portraits/{fileName}";
        }
    }
}