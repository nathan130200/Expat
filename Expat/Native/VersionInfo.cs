﻿using System.Runtime.InteropServices;

namespace Expat.Native;

[StructLayout(LayoutKind.Sequential)]
public readonly struct VersionInfo
{
    public readonly int Major;
    public readonly int Minor;
    public readonly int Build;
}