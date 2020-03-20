using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace fprDump.fpr
{
    public class FprDumper : IDisposable
    {
        private ZipArchive Archive { get; set; }
        public FprDumper(string fprPath)
        {
            Archive = new ZipArchive(File.OpenRead(fprPath), ZipArchiveMode.Read, false);
        }

        public void ExportSource(string outputDirectory)
        {
            XDocument index;
            using (var str = Archive.GetEntry("src-archive/index.xml").Open())
            {
                index = XDocument.Load(str);
            }
            foreach (var entry in index.Descendants("entry"))
            {
                using (var str = Archive.GetEntry(entry.Value).Open())
                using (var reader = new StreamReader(str))
                {
                    var content = reader.ReadToEnd();
                    var name = entry.Attribute(XName.Get("key")).Value;//.Value.Split("/").Last();
                    name = name.Replace(":", "").Replace("..", "_");
                    var path = Path.Combine(outputDirectory, name);
                    Directory.CreateDirectory(path.Substring(0, path.LastIndexOf("/")));
                    File.WriteAllText(path, content);
                }
            }
        }

        public void ExportFindings(string outputDirectory)
        {
            XDocument index;
            using (var str = Archive.GetEntry("audit.fvdl").Open())
            {
                index = XDocument.Load(str);
            }
            var results = new List<string>();
            foreach (var vuln in index.XPathSelectElements("//*[local-name()='Vulnerability']"))
            {
                var vuln_type = vuln.XPathSelectElement(".//*[local-name()='Type']").Value;
                var vuln_location = vuln.XPathSelectElement(".//*[local-name()='SourceLocation']");
                var vuln_location_path = vuln_location.Attribute(XName.Get("path")).Value;
                var vuln_location_lines = (vuln_location.Attribute(XName.Get("line")).Value, vuln_location.Attribute(XName.Get("lineEnd")).Value);
                var vuln_snippet_id = vuln_location.Attribute(XName.Get("snippet"))?.Value;

                if (vuln_location_lines.Item1 == vuln_location_lines.Item2)
                    results.Add($"{vuln_type} at line {vuln_location_lines.Item1} of {vuln_location_path}");
                else
                    results.Add($"{vuln_type} at lines {vuln_location_lines.Item1}-{vuln_location_lines.Item2} of {vuln_location_path}");


                if (!string.IsNullOrEmpty(vuln_snippet_id))
                {
                    var snippet = index.XPathSelectElements("//*[local-name()='Snippet']").FirstOrDefault(x => x.Attribute(XName.Get("id")).Value == vuln_snippet_id).XPathSelectElement(".//*[local-name()='Text']").Value;
                    //Console.WriteLine(snippet);
                }
            }

            results = results.Distinct().OrderBy(x => x).ToList();

            results.ForEach(x => Console.WriteLine(x));
        }

        public void Dispose()
        {
            Archive.Dispose();
        }
    }
}
