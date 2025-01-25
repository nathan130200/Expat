using System.Runtime.InteropServices;

namespace Expat;

public static class Utilities
{
    public static string GetMessage(this ExpatParserError error)
    {
        var cstr = Native.XML_ErrorString(error);
        return Marshal.PtrToStringAnsi(cstr);
    }

#nullable enable

    internal static T? Get<T>(this GCHandle handle) where T : class
        => handle.IsAllocated ? handle.Target as T : null;

#nullable disable
}
