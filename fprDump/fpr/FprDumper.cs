using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace fprDump.fpr
{
    public class FprDumper : IDisposable
    {
        private ZipArchive Archive { get; set; }
        public FprDumper(string fprPath)
        {
            Archive = new ZipArchive(File.OpenRead(fprPath), ZipArchiveMode.Read, false);
        }

        public void ExportSolution(string outputDirectory)
        {
            XDocument index;
            using (var str = Archive.GetEntry("src-archive/index.xml").Open())
            {
                index = XDocument.Load(str);
            }
            foreach(var entry in index.Descendants("entry"))
            {
                using (var str = Archive.GetEntry(entry.Value).Open())
                using (var reader = new StreamReader(str))
                {
                    var content = reader.ReadToEnd();
                    var name = entry.Attribute(XName.Get("key")).Value.Split("/").Last();
                    File.WriteAllText(Path.Combine(outputDirectory, name), content);
                }
            }
        }

        public void Dispose()
        {
            Archive.Dispose();
        }
    }
}
