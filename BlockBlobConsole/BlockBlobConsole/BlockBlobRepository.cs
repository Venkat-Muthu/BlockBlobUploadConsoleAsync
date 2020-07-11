using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace BlockBlobConsole
{
    public class BlockBlobRepository : IBlockBlobRepository
    {
        private readonly string _blobName;
        private readonly BlockBlobClient _blockBlobClient;

        public BlockBlobRepository(IBlockBlobClientFactory blockBlobRepositoryFactory, string containerName,
            string blobName)
        {
            _blobName = blobName;
            _blockBlobClient = blockBlobRepositoryFactory.Create(containerName, blobName);
        }

        public async Task<Response<BlockInfo>> UploadNextFragment(BlockFragment blockFragment)
        {
            await using var stream = new MemoryStream(blockFragment.Buffer);

            using var md5 = MD5.Create();
            var computeHash = md5.ComputeHash(stream);
            stream.Position = 0;
            var base64BlockId = blockFragment.FileFragmentId.ToBase64();
            Response<BlockInfo> stageBlock = null;
            try
            {
                var stageBlockAsync = _blockBlobClient.StageBlockAsync(base64BlockId, stream, computeHash);
                stageBlock = await stageBlockAsync;
                Console.WriteLine($"{stageBlock.GetRawResponse().Status}", "{blockFragment.FileFragmentId:d3}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error : {_blobName}, {blockFragment.FileFragmentId:d3} : {base64BlockId}");
            }

            return stageBlock;
        }

        public async Task<bool> CommitFragments()
        {
            Console.WriteLine("Committing...");
            var blockList = await _blockBlobClient.GetBlockListAsync();
            var blobBlockIds = blockList.Value.UncommittedBlocks
                .Select(item => item.Name).ToList();

            Console.WriteLine($"Found {blobBlockIds.Count} uncommitted blocks");
            await _blockBlobClient.CommitBlockListAsync(
                OrderBlobBlockIds(blobBlockIds)
            );
             return await Task.FromResult(true);
        }

        private IEnumerable<string> OrderBlobBlockIds(IEnumerable<string> blobBlockIds)
        {
            return blobBlockIds.Select(Convert.FromBase64String)
                .Select(Encoding.UTF8.GetString)
                .Select(long.Parse)
                .OrderBy(_ => _)
                .Select(GenerateBlobBlockId)
                .ToArray();
        }
        private string GenerateBlobBlockId(long blockId)
        {
            return blockId.ToString("d20").ToBase64();
        }
    }
}