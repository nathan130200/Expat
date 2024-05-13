using System.IO.Compression;
using Expat;
using Expat.Native;

unsafe
{
    var info = PInvoke.GetExpatVersion();
    Console.WriteLine("GetExpatVersion(): {0}.{1}.{2}", info.Major, info.Minor, info.Build);

    PInvoke.GetExpatVersionString(out var version);
    Console.WriteLine("GetExpatVersionString(): " + version);

    var archive = ZipFile.OpenRead("zipfile.zip");

    using var parser = new Parser();

    int depth = 0;

    parser.OnElementStart += (name, attrs) =>
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string(' ', depth) + "Element: " + name);
        depth += 2;

        Console.ForegroundColor = ConsoleColor.Blue;

        foreach (var (key, value) in attrs)
            Console.WriteLine(new string(' ', depth) + "Attribute: {0} -> {1}", key, value);

        depth += 2;
    };

    parser.OnText += value =>
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(new string(' ', depth) + "Text: " + value.Trim());
    };

    parser.OnCdata += value =>
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(new string(' ', depth) + "Text: " + value);
    };

    parser.OnComment += value =>
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(new string(' ', depth) + "Text: " + value);
    };

    parser.OnElementEnd += name =>
    {
        depth -= 4;
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(new string(' ', depth) + "EndElement: " + name);
    };

    // simulate long IO operation
    var buf = new byte[8];
    int c;

    foreach (var entry in archive.Entries)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Parsing file: '{0}' ...", entry.FullName);

        using var ms = entry.Open();

        while (true)
        {
            c = ms.Read(buf);
            parser.Write(buf, c, c == 0);

            if (c == 0)
                break;
        }

        parser.Reset();
    }
}