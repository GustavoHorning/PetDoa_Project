namespace PetDoa.Services.Interfaces
{
  public interface IFileStorageService
  {
    Task<string?> UploadFileAsync(string blobContainerName, Stream content, string contentType, string fileName);

    Task DeleteFileAsync(string blobContainerName, string fileName);
  }
}
