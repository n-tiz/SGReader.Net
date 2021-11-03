using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace SGReader.Core
{
    public class SGIndex
    {
        public const int IndexSize = 600;
        public const int EntriesCount = 300;

        private readonly ushort[] _entries = new ushort[EntriesCount];

        public ushort Get(int entryId) => _entries[entryId];

        public IReadOnlyCollection<ushort> Entries => _entries;

        public SGIndex(BinaryReader reader)
        {
            for (int i = 0; i < EntriesCount; i++)
            {
                _entries[i] = reader.ReadUInt16();
            }
        }
    }
}