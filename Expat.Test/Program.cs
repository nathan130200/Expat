using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Expat.Test;

internal class Program
{
    static async Task Main(string[] args)
    {
        await Task.Yield();

        using (var parser = new Parser())
        {
            int depth = 0;

            parser.OnElementStart += (s, e) =>
            {
                Console.WriteLine(new string(' ', depth++)
                    + "Element: " + e.TagName);

                if (e.Attributes.Any())
                {
                    var maxPad = e.Attributes.Select(x => x.Key).Max(x => x.Length);

                    depth += 2;

                    foreach (var (name, value) in e.Attributes)
                        Console.WriteLine(new string(' ', depth) + "Attribute: name={0}, value={1}", name.PadLeft(maxPad), value);

                    depth -= 2;
                }
            };

            parser.OnText += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(e.Value))
                    return;

                Console.WriteLine(new string(' ', depth) + "Text: " + HttpUtility.UrlEncode(e.Value));
            };

            parser.OnComment += (s, e) =>
            {
                Console.WriteLine(new string(' ', depth) + "Comment: " + e.Value);
            };

            parser.OnCdata += (s, e) =>
            {
                Console.WriteLine(new string(' ', depth) + "Cdata: " + e.Value
                    .Replace("\n", @"\n")
                    .Replace("\r", @"\r")
                    .Replace("\f", @"\f")
                    .Replace("\t", @"\t"));
            };

            parser.OnProcessingInstruction += (s, e) =>
            {
                Console.WriteLine("ProcessingInstruction: target={0}, data={1}", e.Target, e.Data);
            };

            parser.OnElementEnd += (s, e) =>
            {
                Console.WriteLine(new string(' ', --depth) + "EndElement: {0}", e.TagName);
            };

            parser.OnProlog += (s, e) =>
            {
                Console.WriteLine("Prolog: version={0}, encoding={1}, standalone={2}", e.Version,
                    e.Encoding, e.Standalone);
            };

            try
            {
                var el = new XElement("root");

                for (int i = 0; i < 4; i++)
                {
                    var child = new XElement("item");
                    child.Add(new XAttribute("index", i));
                    child.Add(new XAttribute("hash", child.GetHashCode()));
                    el.Add(child);
                }

                string text = new XDeclaration("1.0", "utf-8", null) + "" +
                    new XProcessingInstruction("foo", "bar") + "" +
                    el.ToString(SaveOptions.DisableFormatting);

                var test = Encoding.UTF8.GetBytes(text);
                parser.Update(test, test.Length);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(ex);
            }
        }

        Console.ReadKey();
    }
}