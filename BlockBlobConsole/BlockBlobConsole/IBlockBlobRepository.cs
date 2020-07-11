using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;

namespace BlockBlobConsole
{
    public interface IBlockBlobRepository
    {
        Task<Response<BlockInfo>> UploadNextFragment(BlockFragment blockFragment);
        Task<bool> CommitFragments();
    }
}