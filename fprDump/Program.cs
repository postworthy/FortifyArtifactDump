using fprDump.fpr;
using System;

namespace fprDump
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var dumper = new FprDumper(args[0]))
            {
                dumper.ExportSolution(args[1]);
            }
        }
    }
}
