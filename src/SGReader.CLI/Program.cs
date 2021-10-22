using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                var loader = new SGLoader();
                loader.Load(fileName);
            }
        }
    }
}
