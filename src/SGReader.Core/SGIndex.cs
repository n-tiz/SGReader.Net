using System.Collections.Generic;
using System.IO;

namespace SGReader.Core
{
    public class SGIndex
    {
        public const int IndexSize = SGFormat.IndexSize;
        public const int EntriesCount = SGFormat.IndexEntryCount;

        private readonly ushort[] _entries = new ushort[EntriesCount];

        public ushort Get(int entryId) => _entries[entryId];

        public IReadOnlyCollection<ushort> Entries => _entries;

        public SGIndex(BinaryReader reader)
        {
            for (int i = 0; i < EntriesCount; i++)
                _entries[i] = reader.ReadUInt16();
        }
    }
}
