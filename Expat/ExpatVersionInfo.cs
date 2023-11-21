using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Expat;

[StructLayout(LayoutKind.Sequential, Size = 12)]
// SemVer compatible
public struct ExpatVersionInfo
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public readonly int Major;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public readonly int Minor;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public readonly int Micro;

    public override readonly string ToString()
        => $"{Major}.{Minor}.{Micro}";
}
