using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BlockBlobConsole
{
    public class ConsoleApplication : IDisposable
    {
        private IDisposable _disposable;
        private readonly IBlockBlobClientFactory _blockBlobClientFactory;
        private IBlockBlobRepository _blockBlobRepository;
        private List<Task> _tasks = new List<Task>();
        private long _fragementUploadInProgress;
        private long _fragementUploadCalled;
        private BlockingCollection<Task> _blockingCollection = new BlockingCollection<Task>();
        private ManualResetEventSlim _manualResetEventSlim = new ManualResetEventSlim();
        public ConsoleApplication(IServiceProvider provider)
        {
            _blockBlobClientFactory = provider.GetService<IBlockBlobClientFactory>();
        }
        public void Run(string containerName, string relativePath, string filename)
        {
            _blockBlobRepository = new BlockBlobRepository(_blockBlobClientFactory, containerName, filename);
            var blockBlobBufferGenerator = new BlockBlobBufferGenerator(relativePath, filename);
            _disposable = blockBlobBufferGenerator.FragmentGenerated.Subscribe(OnNextFragment, OnError, OnCompleted);

            blockBlobBufferGenerator.GenerateStream().ConfigureAwait(false).GetAwaiter();
        }

        private void OnCompleted()
        {
            _manualResetEventSlim.Set();
        }

        private void OnError(Exception obj)
        {
            Console.WriteLine(obj.Message);
        }

        private void OnNextFragment(BlockFragment blockFragment)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    Console.WriteLine(blockFragment.FileFragmentId);
                    var uploadNextFragment = _blockBlobRepository.UploadNextFragment(blockFragment);
                    while (!_blockingCollection.TryAdd(uploadNextFragment)){}

                    if (blockFragment.IsLastFragment)
                    {
                        _manualResetEventSlim.Wait();
                        await Task.WhenAll(_blockingCollection.ToArray());
                        await _blockBlobRepository.CommitFragments();
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            _manualResetEventSlim.Dispose();
            _disposable.Dispose();
            _disposable = null;
        }
        public void Dispose()
        {

            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
        ~ConsoleApplication() => Dispose(false);
    }
}