namespace Expat;
using System.Runtime.InteropServices;
using static Expat.PInvoke;

public class ExpatException : Exception
{
    public XmlError Code { get; init; }

    internal ExpatException(XmlError code, string msg) : base(msg)
    {
        Code = code;
    }

    internal ExpatException(XmlError code) : this(code, GetErrorString(code))
    {

    }

    internal static string GetErrorString(XmlError code)
    {
        nint buf = XML_ErrorString(code);
        return Marshal.PtrToStringAnsi(buf);
    }
}
