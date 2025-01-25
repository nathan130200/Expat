using System.Text;

namespace Expat.Test;

[TestClass]
public class ParseTest
{
    [TestMethod]
    public void BasicParsingTest()
    {
        var bytes = "<foo xmlns='bar '/>"u8.ToArray();

        using var parser = CreateParser();
        var result = parser.TryParse(bytes, bytes.Length, out var error, true);
        Console.WriteLine("(parser) result: " + result);
        Console.WriteLine("(parser) error: " + error);

        Assert.IsTrue(result);
        Assert.AreEqual(ExpatParserError.None, error);
    }

    public ExpatParser CreateParser(bool bindEvents = true, bool strict = false)
    {
        var parser = new ExpatParser(ExpatEncoding.UTF8, strict);

        if (bindEvents)
        {
            parser.OnStartElement += (name, attrs) =>
            {
                Console.WriteLine($"StartTag: {name}");

                foreach (var (key, value) in attrs)
                    Console.WriteLine($" Attribute: {key}={value}");
            };

            parser.OnCdata += value => Console.WriteLine("Cdata: " + value);
            parser.OnComment += value => Console.WriteLine("Comment: " + value);
            parser.OnText += value => Console.WriteLine("Text: " + value);
            parser.OnEndElement += tag => Console.WriteLine("EndTag: " + tag);
        }

        return parser;
    }

    [TestMethod]
    public void BasicParsingTestMayFail()
    {
        var bytes = "<foo xmlns='bar '><baz/></foo>"u8.ToArray();

        using var parser = CreateParser();

        parser.Parse(bytes, bytes.Length, true);

        var ex = Assert.ThrowsException<ExpatException>(() =>
        {
            parser.Parse(bytes, bytes.Length, true);
        });

        Console.WriteLine("Line Info: " + ex.LineNumber + "; " + ex.LinePosition + " (code: " + ex.Code + ")");
    }

    [TestMethod]
    public void ParseChunkedXml()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(@"<?xml version=""1.0"" encoding=""UTF-8"" ?><abstract-root><shoulder gift=""chose"">rod</shoulder><carbon over=""gulf"">1508782430</carbon><football vessels=""colony""><color cloud=""own""><press>369355735<![CDATA[planned fox north touch]]></press><important refer=""cook""><thou idea=""student"">-1997937505.530799</thou><!--discover--><anywhere>-1253233402</anywhere><to><!--jack-->1540951237.4368534</to><education point=""kept"">1850007192.8730412</education><!--speak found--><!--prepare local pattern--><worth>994658426.3385849</worth><worse gas=""change""><!--recent-->dinner</worse><author>1441715282</author></important><!--wool identity arrow--><figure>bound<![CDATA[base desert therefore club report]]></figure><noun>1285020904.7518005</noun><warn tonight=""feed"">-1735516686.1591387</warn><pig feel=""stop"">243392692<!--attempt against highway post sea cry--></pig><go>-52976231</go></color><fact>1080154555.5196805</fact><massage>1593228701.2733817</massage><eventually>549097759.6130838</eventually><declared former=""carry"">33660956</declared><![CDATA[long]]><cool myself=""show"">-645385897.1654816</cool><!--having vertical--><hope>-902136102</hope></football><![CDATA[seeing]]><mountain farmer=""hello"">flower</mountain><waste>coal</waste><slabs thousand=""solve"">-512372303</slabs><business>history<!--happened--></business></abstract-root>"));

        using var parser = CreateParser();

        var buf = new byte[256];

        // simulate long IO operation
        while (true)
        {
            int len = stream.Read(buf);

            if (len <= 0)
                break;

            parser.Parse(buf, len, false);
        }

        parser.Parse(buf, 0, true);
    }

    [TestMethod]
    public void XmlAttackTest()
    {
        var bytes = @"<?xml version=""1.0""?>
<!DOCTYPE lolz [
<!ENTITY lol ""lol"">
<!ELEMENT lolz (#PCDATA)>
<!ENTITY lol1 ""&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;"">
<!ENTITY lol2 ""&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;"">
<!ENTITY lol3 ""&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;"">
<!ENTITY lol4 ""&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;"">
<!ENTITY lol5 ""&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;"">
<!ENTITY lol6 ""&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;"">
<!ENTITY lol7 ""&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;"">
<!ENTITY lol8 ""&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;"">
<!ENTITY lol9 ""&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;"">
]>
<root>&nbsp;&lt;&gt;&quote;&apos;&amp;<lolz>&lol9;</lolz></root>"u8.ToArray();

        using var parser = CreateParser(false, true);
        var result = parser.TryParse(bytes, bytes.Length, out var error);

        Console.WriteLine("result: " + result);
        Console.WriteLine("error: " + error);

        Assert.IsFalse(result);
        Assert.AreEqual(ExpatParserError.Aborted, error);

    }
}
