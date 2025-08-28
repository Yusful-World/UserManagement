namespace UserManagement.Infrastructure.Services.Interfaces
{
    public interface IImageService
    {
        public bool IsValidImage(IFormFile image);
        public string ValidateStringUrl(string url);
        public Task<string> SaveImageAsync(IFormFile image);
        public Task DeleteImageAsync(string imageUrl);
    }
}
