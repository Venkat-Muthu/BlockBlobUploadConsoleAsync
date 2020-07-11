using Azure.Storage.Blobs.Specialized;

namespace BlockBlobConsole
{
    public interface IBlockBlobClientFactory
    {
        BlockBlobClient Create(string blobContainerName, string blobName);
    }
}