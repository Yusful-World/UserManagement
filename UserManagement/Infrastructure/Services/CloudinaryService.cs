using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using UserManagement.Infrastructure.Services.Interfaces;

namespace UserManagement.Infrastructure.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudinarySettings = configuration.GetSection("Cloudinary");
            var account = new Account(
                configuration["CLOUDINARY_CLOUD_NAME"],
                configuration["CLOUDINARY_API_KEY"],
                configuration["CLOUDINARY_API_SECRET"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be empty", nameof(fileName));

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                PublicId = $"business_logos/{Path.GetFileNameWithoutExtension(fileName)}"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }

        public async Task DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}
