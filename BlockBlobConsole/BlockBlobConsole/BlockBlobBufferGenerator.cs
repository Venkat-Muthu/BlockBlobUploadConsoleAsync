using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace BlockBlobConsole
{
    public class BlockBlobBufferGenerator
    {
        private readonly string _relativePath;
        private readonly string _fileName;

        private readonly Subject<BlockFragment> _fragmentGenerated = new Subject<BlockFragment>();
        public IObservable<BlockFragment> FragmentGenerated => _fragmentGenerated.AsObservable();

        public BlockBlobBufferGenerator(string relativePath, string fileName)
        {
            _relativePath = relativePath;
            _fileName = fileName;
        }

        public async Task GenerateStream()
        {
            await Task.Factory.StartNew(async () =>
            {
                var blockFragmentSize = (int) (0.5 * 1000.0 * 1024.0);
                var readAllBytes = await File.ReadAllBytesAsync(Path.Combine(_relativePath, _fileName));
                var totalLength = readAllBytes.Length;
                var fragmentCount = (int) Math.Ceiling((double)totalLength / blockFragmentSize);
                for (var i = 0; i < fragmentCount; i++)
                {   
                    var isLastFragment = false;
                    var sourceIndex = i * blockFragmentSize;
                    int lengthToCopy;
                    byte[] blockByteArray;
                    if (sourceIndex + blockFragmentSize > totalLength)
                    {
                        lengthToCopy = totalLength - sourceIndex;
                        blockByteArray = new byte[lengthToCopy];
                        isLastFragment = true;
                    }
                    else
                    {
                        blockByteArray = new byte[blockFragmentSize];
                        lengthToCopy = blockFragmentSize;
                    }
                    Console.WriteLine($"Total : {totalLength}, Index : {i}, Copying : {sourceIndex}..{sourceIndex+ lengthToCopy}");
                    var blockName = $"{Path.GetFileNameWithoutExtension(_fileName)}-{i}.{Path.GetExtension(_fileName)}";
                    Array.Copy(readAllBytes, sourceIndex, blockByteArray, 0, lengthToCopy);
                    FragmentGeneratedOnTheFly(new BlockFragment(_fileName, i, blockByteArray, blockName, isLastFragment));
                    //await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            });
        }

        public void FragmentGeneratedOnTheFly(BlockFragment blockFragment)
        {
            try
            {
                _fragmentGenerated.OnNext(blockFragment);
                if (blockFragment.IsLastFragment)
                {
                    _fragmentGenerated.OnCompleted();
                }
            }
            catch (Exception exception)
            {
                _fragmentGenerated.OnError(exception);
            }
        }
    }
}