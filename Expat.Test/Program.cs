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
                // XML generated using https://codebeautify.org/generate-random-xml
                // used for testing all possible events in expat parser.

                parser.Reset();

                var sample = Encoding.UTF8.GetBytes(@"<?xml version=""1.0"" encoding=""UTF-8"" ?><root><j4pSpl6r17-E3E stopped=""shot""><ZLIXTS9oY23T1Z7F neighborhood=""flag""><![CDATA[trap]]><So><nQODMLpRM-gCMIuzlA2Oe fairly=""careful"">1308615073.4641433</nQODMLpRM-gCMIuzlA2Oe><i describe=""disease"">-143351890</i></So><n><q sang=""twice"">-1065943463.8838549</q><![CDATA[correct]]><ueVO4y25LSdug>Rv"")5[.P3</ueVO4y25LSdug></n><!--problem chain region family--></ZLIXTS9oY23T1Z7F><L0o3lRmMMs03KIqS>-591225543.8601017</L0o3lRmMMs03KIqS></j4pSpl6r17-E3E><![CDATA[come]]><t6YzcOA4bJ-wVOSbZn><XUp-mkT buy=""far"">W&amp;7oo//U/&amp;JZc4TmW6]5xEn</XUp-mkT><wFssAyhdCGJaBju02 after=""welcome""><mp9Ub_Sq><o7PBrANNkbUVyEe>X8i</o7PBrANNkbUVyEe><!--breakfast tea needle--><muhS7hYaSwm>-237605011.8712139</muhS7hYaSwm></mp9Ub_Sq><UOUi0pnr parts=""careful""><!--stared so hello-->-697712748<![CDATA[flower repeat our anyone six amount seems]]></UOUi0pnr></wFssAyhdCGJaBju02></t6YzcOA4bJ-wVOSbZn></root>");

                parser.Update(sample, sample.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        Console.ReadKey();
    }
}