using System.Text;
using Expat.Native;

namespace Expat.Test;

[TestClass]
public class StringParserTest
{
    [TestMethod]
    public void ShowExpatVersion()
    {
        try
        {
            PInvoke.GetExpatVersionString(out var str);
            Console.WriteLine(str);

            var info = PInvoke.GetExpatVersion();
            Console.WriteLine("major={0}", info.Major);
            Console.WriteLine("minor={0}", info.Minor);
            Console.WriteLine("build={0}", info.Build);
        }
        catch (Exception e)
        {
            Assert.Fail(e.ToString());
        }
    }

    [TestMethod]
    [DataRow("<foo xmlns='bar'/>")]
    [DataRow("<foo xmlns='baz'><bar/></foo>")]
    [DataRow("<foo xmlns='baz'><bar>content node</bar></foo>")]
    [DataRow("<foo xmlns='baz'><!-- a comment --></foo>")]
    [DataRow("<foo xmlns='baz'><![CDATA[ a data ]]></foo>")]
    [DataRow("<foo xmlns='bar'/>")]
    [DataRow("<foo xmlns='baz'><bar/><!-- a comment --></foo>")]
    [DataRow("<foo xmlns='baz'><bar>content node</bar><![CDATA[ a data ]]></foo>")]
    public void ParseFromString(string xml)
    {
        var buf = new byte[4];
        var cnt = 0;

        using var ms = new MemoryStream();
        ms.Write(Encoding.UTF8.GetBytes(xml));
        ms.Position = 0;

        using var parser = new Parser();

        while ((cnt = ms.Read(buf)) > 0)
        {
            parser.Write(buf, cnt, false);
        }

        parser.Write(buf, 0, true);
    }

    [TestMethod]
    public void ParseAfterDisposed()
    {
        Parser p;
        {
            p = new();
            p.Dispose();
        }
    }
}