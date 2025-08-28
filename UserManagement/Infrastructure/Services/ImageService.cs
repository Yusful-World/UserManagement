using UserManagement.Infrastructure.Services.Interfaces;

namespace UserManagement.Infrastructure.Services
{
    public class ImageService : IImageService 
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ImageService> _logger;
        private readonly ICloudinaryService _cloudinaryService;

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private static readonly string[] AllowedDocExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };

        public ImageService(ILogger<ImageService> logger, IConfiguration configuration, ICloudinaryService cloudinaryService)
        {
            _configuration = configuration;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }

        public string ValidateStringUrl(string url)
        {

            var extension = Path.GetExtension(url)?.ToLower();
            return AllowedImageExtensions.Contains(extension)
                ? string.Empty
                : "Invalid image Url. Only .jpg, .jpeg, .png, .gif are allowed.";
        }

        public bool IsValidImage(IFormFile image) =>
        AllowedImageExtensions.Contains(Path.GetExtension(image.FileName).ToLower());

        public async Task<string> SaveImageAsync(IFormFile image)
        {
            if (!IsValidImage(image))
                throw new InvalidOperationException("Invalid file type. Only JPG, JPEG, PNG, GIF files are allowed.");

            using var stream = image.OpenReadStream();
            var storePath = await _cloudinaryService.UploadImageAsync(stream, image.Name);
            _logger.LogWarning("IMAGE_STORE not configured. Falling back to default path: {Path}", storePath);

            return await Task.FromResult(storePath);
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentException("Image URL cannot be null or empty.", nameof(imageUrl));
            }

            await _cloudinaryService.DeleteImageAsync(imageUrl);

            await Task.CompletedTask;

        }
    }
}
