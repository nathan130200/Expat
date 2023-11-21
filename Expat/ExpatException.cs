namespace Expat;
using System.Runtime.InteropServices;
using static Expat.PInvoke;

public class ExpatException : Exception
{
    public ErrorCode Code { get; init; }

    internal ExpatException(ErrorCode code, string msg) : base(msg)
    {
        Code = code;
    }

    internal ExpatException(ErrorCode code) : this(code, GetErrorString(code))
    {

    }

    internal static string GetErrorString(ErrorCode code)
    {
        nint buf = XML_ErrorString(code);
        return Marshal.PtrToStringAnsi(buf);
    }
}
