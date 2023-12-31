﻿using System.Web;

namespace Expat.Test;

internal class Program
{
    static void Main(string[] args)
    {
        // using vcpkg helper to resolve libexpat location.
        LibraryResolver.UseVcpkg(@"C:\VCPKG", "x64-windows");

        using (var parser = new Parser())
        {
            int depth = 0;

            parser.OnElementStart += (e) =>
            {
                Console.WriteLine(new string(' ', depth++)
                    + "Element: " + e.Name);

                if (e.Attributes.Any())
                {
                    var maxPad = e.Attributes.Select(x => x.Key).Max(x => x.Length);

                    depth += 2;

                    foreach (var (name, value) in e.Attributes)
                        Console.WriteLine(new string(' ', depth) + "Attribute: name={0}, value={1}", name.PadLeft(maxPad), value);

                    depth -= 2;
                }
            };

            parser.OnText += (e) =>
            {
                if (string.IsNullOrWhiteSpace(e.Value))
                    return;

                Console.WriteLine(new string(' ', depth) + "Text: " + HttpUtility.UrlEncode(e.Value));
            };

            parser.OnComment += (e) =>
            {
                Console.WriteLine(new string(' ', depth) + "Comment: " + e.Value);
            };

            parser.OnCdata += (e) =>
            {
                Console.WriteLine(new string(' ', depth) + "Cdata: " +
                    e.Value
                    .Replace("\n", @"\n")
                    .Replace("\r", @"\r")
                    .Replace("\f", @"\f")
                    .Replace("\t", @"\t"));
            };

            parser.OnProcessingInstruction += (e) =>
            {
                Console.WriteLine("ProcessingInstruction: target={0}, data={1}", e.Target, e.Data);
            };

            parser.OnElementEnd += (e) =>
            {
                Console.WriteLine(new string(' ', --depth) + "EndElement: {0}", e.Value);
            };

            parser.OnProlog += (e) =>
            {
                Console.WriteLine("Prolog: version={0}, encoding={1}, standalone={2}", e.Version,
                    e.Encoding.WebName, e.Standalone);
            };

            try
            {
                var fileName = Path.Combine(Directory.GetCurrentDirectory(), "sample.xml");
                using var fs = File.OpenRead(fileName);

                var buf = new byte[8];
                int count;

                // simulate long I/O operation (eg.: network socket)

                while ((count = fs.Read(buf)) > 0)
                    parser.Feed(buf, count, fs.Position != fs.Length - 1);

                parser.Feed(buf, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        Console.ReadKey();
    }
}