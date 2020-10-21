using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace BlobStorage
{
    public class BlobService
    {
        private BlobServiceClient _blobServiceClient;
        public BlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }
        public async Task UploadFileFromStreamAsync(Stream fileStream,string fileName,string containerName)
        {
            var blobContainerClient = await CreateOrGetBlobContainerClient(containerName);
            await blobContainerClient.DeleteBlobIfExistsAsync(fileName);
            await blobContainerClient.UploadBlobAsync(fileName, fileStream);
        }

        public async Task<BlobContainerClient> CreateOrGetBlobContainerClient(string containerName)
        {
            try
            {
                var azureResponse = await _blobServiceClient.CreateBlobContainerAsync(containerName);
                return azureResponse.Value;
            }
            catch (Azure.RequestFailedException)
            {
                return _blobServiceClient.GetBlobContainerClient(containerName);
            }

        }
    }
}