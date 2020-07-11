namespace BlockBlobConsole
{
    public class BlockFragment
    {
        public string FileUniqueId { get; private set; }
        public long FileFragmentId { get; private set; }
        public byte[] Buffer { get; private set; }
        public string BlockName { get; private set; }
        public bool IsLastFragment { get; private set; }

        public BlockFragment(string fileUniqueId, long fileFragmentId, byte[] buffer, string blockName,
            bool isLastFragment)
        {
            FileUniqueId = fileUniqueId;
            FileFragmentId = fileFragmentId;
            Buffer = buffer;
            BlockName = blockName;
            IsLastFragment = isLastFragment;
        }
    }
}