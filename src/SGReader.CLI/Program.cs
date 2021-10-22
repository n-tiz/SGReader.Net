using System.Linq;
using SGReader.Core;

namespace SGReader.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                string fileName = args.SingleOrDefault();
                var sgFile = new SGFile(fileName);
                sgFile.Load();
            }
        }
    }
}
