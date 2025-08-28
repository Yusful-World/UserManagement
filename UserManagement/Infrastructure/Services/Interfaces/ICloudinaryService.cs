namespace UserManagement.Infrastructure.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(Stream file, string fileName);
        Task DeleteImageAsync(string publicId);
    }
}
