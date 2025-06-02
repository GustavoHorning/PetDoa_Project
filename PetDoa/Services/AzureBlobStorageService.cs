using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PetDoa.Services.Interfaces;

namespace PetDoa.Services
{
  public class AzureBlobStorageService : IFileStorageService
  {
    private readonly string _connectionString;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(IConfiguration configuration , ILogger<AzureBlobStorageService> logger)
    {
      _logger = logger;
      _connectionString = configuration.GetValue<string>("AzureStorage:ConnectionString")
                         ?? throw new InvalidOperationException("AzureStorage:ConnectionString não configurada.");

      if (string.IsNullOrEmpty(_connectionString))
      {
        _logger.LogError("Azure Storage Connection String está vazia ou nula.");
        throw new InvalidOperationException("Azure Storage Connection String não pode ser vazia.");
      }

      _blobServiceClient = new BlobServiceClient(_connectionString);
      _logger.LogInformation("AzureBlobStorageService inicializado.");
    }

    private async Task<BlobContainerClient> GetContainerClient(string blobContainerName)
    {
      var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
      try
      {
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao tentar criar/verificar contêiner '{ContainerName}'", blobContainerName);
        throw;
      }
      return containerClient;
    }


    public async Task<string?> UploadFileAsync(string blobContainerName, Stream content, string contentType, string fileName)
    {
      if (content == null || content.Length == 0)
      {
        _logger.LogWarning("Tentativa de upload de arquivo vazio ou nulo para {FileName} em {ContainerName}", fileName, blobContainerName);
        return null;
      }

      try
      {
        var containerClient = await GetContainerClient(blobContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        _logger.LogInformation("Iniciando upload para {BlobUri}", blobClient.Uri);

        await blobClient.UploadAsync(content, new BlobUploadOptions
        {
          HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }); //,overwrite: true); // Adicione overwrite se necessário

        _logger.LogInformation("Upload concluído para {BlobUri}", blobClient.Uri);

        return blobClient.Uri.ToString();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro durante upload para {ContainerName}/{FileName}", blobContainerName, fileName);
        return null;
      }
    }

    public async Task DeleteFileAsync(string blobContainerName, string fileName)
    {
      try
      {
        var containerClient = await GetContainerClient(blobContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        _logger.LogInformation("Tentando deletar blob: {BlobUri}", blobClient.Uri);

        await blobClient.DeleteIfExistsAsync();

        _logger.LogInformation("Blob deletado (se existia): {BlobUri}", blobClient.Uri);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao deletar {ContainerName}/{FileName}", blobContainerName, fileName);
      }
    }
  }
}
