using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace Part3EventEase.Services
{
    public class BlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public BlobService(IConfiguration configuration)
        {
            _connectionString = configuration["AzureBlob:ConnectionString"];
            _containerName = configuration["AzureBlob:ContainerName"];
        }

        public async Task<string?> UploadFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var container = new BlobContainerClient(_connectionString, _containerName);
            await container.CreateIfNotExistsAsync();

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var blob = container.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, overwrite: true);

            return GenerateSasUrl(blob);
        }

        private string GenerateSasUrl(BlobClient blob)
        {
            if (!blob.CanGenerateSasUri)
                return blob.Uri.ToString();

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blob.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddYears(1)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blob.GenerateSasUri(sasBuilder).ToString();
        }
    }
}
