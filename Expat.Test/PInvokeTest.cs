using System.Text;
using Expat.Native;

namespace Expat.Test;

[TestClass]
public class PInvokeTest
{
    [TestMethod]
    public void TestErrorCodes()
    {
        var codes = Enum.GetValues<Error>();

        foreach (var code in codes)
        {
            var str = PInvoke.GetErrorString(code);
            Console.WriteLine("Code: {0} ({1}) -> {2}", code, (int)code, str);
        }
    }

    [TestMethod]
    public void TestParsingState()
    {
        using var parser = new ExpatParser(EncodingType.UTF8);
        Dump(PInvoke.GetParsingStatus(parser.CPointer));

        parser.Write("<foo/>"u8.ToArray(), 6, false);
        Dump(PInvoke.GetParsingStatus(parser.CPointer));

        parser.Write([], 0, true);
        Dump(PInvoke.GetParsingStatus(parser.CPointer));

        void Dump(ParsingState state)
        {
            Console.WriteLine(" === Parser Status ===");
            Console.WriteLine(" Kind: " + state.Status);
            Console.WriteLine(" Is Final: " + state.IsFinalBuffer);
        }
    }

    [TestMethod]
    public void TestParsingInfo()
    {
        var xml = Encoding.UTF8.GetBytes("<foo xmlns='bar'><bar/><bar/><bar/><bar/><bar/><bar/><bar/><bar/><bar/><bar/>" +
            "<bar/><bar/><bar/><bar/><bar/><bar/><bar/><bar/><bar/><bar/><bar/><bar/></foo>");

        using var ms = new MemoryStream(xml)
        {
            Position = 0
        };

        var byteCount = (int)ms.Length;

        using var parser = new ExpatParser(EncodingType.UTF8);
        Dump();

        var buf = new byte[8];

        while (true)
        {
            var count = ms.Read(buf);
            parser.Write(buf, count, count == 0);

            if (count == 0)
                break;

            Dump();
        }

        Assert.ThrowsException<ExpatException>(() =>
        {
            parser.Write([], 0, true);
            Dump();
        });

        void Dump()
        {
            Console.WriteLine("line: {0,3};{1,-3} | byte: {2,3},{3,-3} | r: {4,4:##0.00}%",
                parser.LineNumber, parser.ColumnNumber,
                parser.ByteIndex, parser.ByteCount,
                ((parser.ByteIndex + 1) / (float)(byteCount + 1)) * 100f);
        }
    }

    [TestMethod]
    public void TestBillionLaughsAttack()
    {
        var xml = @"<?xml version=""1.0""?>
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
<lolz>&lol9;</lolz>";

        var buf = Encoding.UTF8.GetBytes(xml);

        using var parser = new ExpatParser();

        try
        {
            parser.Write(buf, buf.Length, false);
            parser.Write([], 0, true);
        }
        catch (ExpatException ex)
        {
            Console.WriteLine("<{0}>: {1}:\n\t{2}", (int)ex.Code, ex.Code, ex.Message);
            Assert.AreEqual(Error.AmplificationLimitBreach, ex.Code);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.ToString());
        }
    }

    [TestMethod]
    public void TestGetAllSupportedFeatures()
    {
        var features = PInvoke.GetFeatureList();

        foreach (var feature in features)
        {
            Console.WriteLine("<{0}> {1}: {2} (value: {3})", (int)feature.Type, feature.Type, feature.Name, feature.Value);
        }
    }
}
