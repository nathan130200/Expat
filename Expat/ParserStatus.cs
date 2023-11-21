using System.Runtime.InteropServices;

namespace Expat;

[StructLayout(LayoutKind.Sequential)]
public struct ParserStatus
{
    public readonly ParserStatusType Type;

    [MarshalAs(UnmanagedType.Bool)]
    public readonly bool IsFinalBuffer;
}
