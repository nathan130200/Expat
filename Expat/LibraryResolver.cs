using System.Reflection;
using System.Runtime.InteropServices;

namespace Expat;

public static class LibraryResolver
{
    internal static readonly Assembly _assembly = typeof(LibraryResolver).Assembly;
    internal static string _libraryFullPath;

    static LibraryResolver()
    {
        NativeLibrary.SetDllImportResolver(_assembly, ResolveNativeLibrary);
    }

    static nint ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != "libexpat")
            return nint.Zero;

        if (string.IsNullOrEmpty(_libraryFullPath))
            return nint.Zero;

        return NativeLibrary.Load(_libraryFullPath);
    }

    public static string GetExtension()
    {
        var result = ".so";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            result = ".dll";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            result = ".dylib";

        return result;
    }

    public static void UseVcpkg(string baseDir, string triplet, bool debugLibraries = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseDir);
        ArgumentException.ThrowIfNullOrEmpty(triplet);

        var extension = GetExtension();
        var binaryDir = !debugLibraries ? "bin" : Path.Combine("debug", "bin");
        var searchPath = Path.Combine(baseDir, "installed", triplet, binaryDir);
        var fileName = !debugLibraries ? "libexpat" : "libexpatd";

        _libraryFullPath = Path.Combine(searchPath, $"{fileName}{extension}");

        if (!File.Exists(_libraryFullPath))
            throw new FileNotFoundException("Expat library not found in vcpkg install path.", _libraryFullPath);
    }

    public static void UseSystemDefault()
        => _libraryFullPath = default;
}
