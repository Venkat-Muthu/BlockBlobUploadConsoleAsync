using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Options;

namespace BlockBlobConsole
{
    public class BlockBlobClientFactory : IBlockBlobClientFactory
    {
        private readonly BlobStorageConfig _blobStorageConfig;
        public BlockBlobClientFactory(IOptions<BlobStorageConfig> blobUtilityOptions)
        {
            _blobStorageConfig = blobUtilityOptions.Value;
        }

        public BlockBlobClient Create(string blobContainerName, string blobName)
        {
            var blobContainerClient = new BlobContainerClient(_blobStorageConfig.ConnectionString, blobContainerName);
            if (!blobContainerClient.Exists())
            {
                blobContainerClient.CreateIfNotExists();
            }
            var blockBlobClient = new BlockBlobClient(_blobStorageConfig.ConnectionString, blobContainerName, blobName);
            return blockBlobClient;
        }
    }
}