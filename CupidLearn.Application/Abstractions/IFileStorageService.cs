namespace CupidLearn.Application.Abstractions;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, string contentType);
    Task DeleteAsync(string fileKey);
}
