namespace SkinPAI.API.Services;

public interface IFileStorageService
{
    Task<string> SaveImageAsync(string base64Image, string folder, string fileName);
    Task<string> SaveFileAsync(byte[] fileBytes, string folder, string fileName);
    Task<bool> DeleteFileAsync(string filePath);
    string GetFileUrl(string relativePath);
}

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _uploadPath;

    public FileStorageService(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
        
        // Set up the uploads directory
        _uploadPath = Path.Combine(_environment.ContentRootPath, "Uploads");
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<string> SaveImageAsync(string base64Image, string folder, string fileName)
    {
        try
        {
            // Remove data URL prefix if present
            var base64Data = base64Image;
            string extension = ".jpg";
            
            if (base64Image.Contains(","))
            {
                var parts = base64Image.Split(',');
                base64Data = parts[1];
                
                // Determine extension from MIME type
                if (parts[0].Contains("png"))
                    extension = ".png";
                else if (parts[0].Contains("gif"))
                    extension = ".gif";
                else if (parts[0].Contains("webp"))
                    extension = ".webp";
            }

            var imageBytes = Convert.FromBase64String(base64Data);
            return await SaveFileAsync(imageBytes, folder, $"{fileName}{extension}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving image: {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> SaveFileAsync(byte[] fileBytes, string folder, string fileName)
    {
        try
        {
            // Create folder if it doesn't exist
            var folderPath = Path.Combine(_uploadPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Generate unique filename
            var uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
            var filePath = Path.Combine(folderPath, uniqueFileName);

            // Save file
            await File.WriteAllBytesAsync(filePath, fileBytes);

            // Return relative path
            var relativePath = Path.Combine(folder, uniqueFileName).Replace("\\", "/");
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", fileName);
            throw;
        }
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_uploadPath, filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    public string GetFileUrl(string relativePath)
    {
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7001";
        return $"{baseUrl}/uploads/{relativePath}";
    }
}
