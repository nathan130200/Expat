using System.Text;

namespace Expat;

public sealed class ExpatEncoding
{
    public static ExpatEncoding ISO88591 { get; } = new("ISO-8859-1", Encoding.Latin1);
    public static ExpatEncoding ASCII { get; } = new("US-ASCII", Encoding.ASCII);
    public static ExpatEncoding UTF8 { get; } = new("UTF-8", Encoding.UTF8);
    public static ExpatEncoding UTF16 { get; } = new("UTF-16", Encoding.Unicode);
    public static ExpatEncoding UTF16LE { get; } = new("UTF-16LE", Encoding.Unicode);
    public static ExpatEncoding UTF16BE { get; } = new("UTF-16BE", Encoding.BigEndianUnicode);

    public string Name { get; }
    public Encoding NativeEncoding { get; }

    ExpatEncoding(string name, Encoding nativeEncoding)
    {
        Name = name;
        NativeEncoding = nativeEncoding;
    }
}
