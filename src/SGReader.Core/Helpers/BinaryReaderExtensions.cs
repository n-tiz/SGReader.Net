using System.IO;

namespace SGReader.Core.Helpers
{
    public static class BinaryReaderExtensions
    {
        public static int[] ReadInts32(this BinaryReader self, int count)
        {
            var array = new int[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = self.ReadInt32();
            }
            return array;
        }

        public static short[] ReadInts16(this BinaryReader self, int count)
        {
            var array = new short[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = self.ReadInt16();
            }
            return array;
        }

        public static void Skip(this BinaryReader self, int bytes) => self.BaseStream.Seek(bytes, SeekOrigin.Current);
    }
}