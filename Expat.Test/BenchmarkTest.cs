using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using System.Text;
using System.Xml.Linq;

namespace Expat.Test;

[MemoryDiagnoser]
[ShortRunJob]
public class BenchmarkTest
{
    static readonly byte[] SampleXML = Encoding.UTF8.GetBytes(@"<?xml version=""1.0"" encoding=""UTF-8"" ?><root><j4pSpl6r17-E3E stopped=""shot""><ZLIXTS9oY23T1Z7F neighborhood=""flag""><![CDATA[trap]]><So><nQODMLpRM-gCMIuzlA2Oe fairly=""careful"">1308615073.4641433</nQODMLpRM-gCMIuzlA2Oe><i describe=""disease"">-143351890</i></So><n><q sang=""twice"">-1065943463.8838549</q><![CDATA[correct]]><ueVO4y25LSdug>Rv"")5[.P3</ueVO4y25LSdug></n><!--problem chain region family--></ZLIXTS9oY23T1Z7F><L0o3lRmMMs03KIqS>-591225543.8601017</L0o3lRmMMs03KIqS></j4pSpl6r17-E3E><![CDATA[come]]><t6YzcOA4bJ-wVOSbZn><XUp-mkT buy=""far"">W&amp;7oo//U/&amp;JZc4TmW6]5xEn</XUp-mkT><wFssAyhdCGJaBju02 after=""welcome""><mp9Ub_Sq><o7PBrANNkbUVyEe>X8i</o7PBrANNkbUVyEe><!--breakfast tea needle--><muhS7hYaSwm>-237605011.8712139</muhS7hYaSwm></mp9Ub_Sq><UOUi0pnr parts=""careful""><!--stared so hello-->-697712748<![CDATA[flower repeat our anyone six amount seems]]></UOUi0pnr></wFssAyhdCGJaBju02></t6YzcOA4bJ-wVOSbZn></root>");

    [Benchmark]
    public void ParseUsingExpat()
    {
        using var parser = new Parser();
        parser.Update(SampleXML, SampleXML.Length);
    }

    [Benchmark]
    public void ParseUsingXElement()
    {
        using var ms = new MemoryStream(SampleXML);
        var el = XElement.Load(ms);
    }
}
