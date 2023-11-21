# Expat
[Expat](https://github.com/libexpat/libexpat) bindings for .NET/C#. 

## About
Expat, a C99 library for parsing [XML 1.0 Fourth Edition](https://www.w3.org/TR/2006/REC-xml-20060816/), started by [James Clark](https://en.wikipedia.org/wiki/James_Clark_%28programmer%29) in 1997. Expat is a stream-oriented XML parser. This means that you register handlers with the parser before starting the parse. These handlers are called when the parser discovers the associated structures in the document being parsed from.

## Bindings
These bindings are intended to provide most of the features available between libexpat and .NET

All P/Invoking functions added can be found in `PInvoke` class that is publicy visible.

## Basic Parser

This bindings already has a simple parser implemented by default, which basically will redirect all of the P/Invoking callbacks and convert in a simple and practical way and will fire the events as needed.

```cs

using Expat;

// other stuff before...

using (var parser = new Parser())
{
    int depth = 0;

    // handle start tag

    parser.OnElementStart += (s, e) =>
    {
        Console.WriteLine(new string(' ', depth++)
            + "Element: " + e.TagName);

        if (e.Attributes.Any())
        {
            var maxPad = e.Attributes.Select(x => x.Key).Max(x => x.Length);

            depth += 2;

            foreach (var (name, value) in e.Attributes)
                Console.WriteLine(new string(' ', depth) + "Attribute: name={0}, value={1}", name.PadLeft(maxPad), value);

            depth -= 2;
        }
    };

    // handle text nodes.

    parser.OnText += (s, e) =>
    {
        if (string.IsNullOrWhiteSpace(e.Value))
            return;

        Console.WriteLine(new string(' ', depth) + "Text: " + HttpUtility.UrlEncode(e.Value));
    };

    // handle comments

    parser.OnComment += (s, e) =>
    {
        Console.WriteLine(new string(' ', depth) + "Comment: " + e.Value);
    };

    // handle CData sections

    parser.OnCdata += (s, e) =>
    {
        Console.WriteLine(new string(' ', depth) + "Cdata: " +
            e.Value
            .Replace("\n", @"\n")
            .Replace("\r", @"\r")
            .Replace("\f", @"\f")
            .Replace("\t", @"\t"));
    };

    // 
    // handle PI
    // 
    //      &lt;?my_key my_data ?&gt;
    // 
    // 
    parser.OnProcessingInstruction += (s, e) =>
    {
        Console.WriteLine("ProcessingInstruction: target={0}, data={1}", e.Target, e.Data);
    };

    // handle end tag
    parser.OnElementEnd += (s, e) =>
    {
        Console.WriteLine(new string(' ', --depth) + "EndElement: {0}", e.TagName);
    };

    // handle xml prolog: version, encoding and standalone

    parser.OnProlog += (s, e) =>
    {
        Console.WriteLine("Prolog: version={0}, encoding={1}, standalone={2}", e.Version,
            e.Encoding, e.Standalone);
    };

    byte[] xmlBytes = ...; // load some XML bytes from file, network socket, etc.

    // feed bytes to the parser.
    var code = parser.Feed(sample, sample.Length);

    // check if was success (code == OK)

    if(code != Result.Success) {
        if(code == Result.Error) {
            var errorCode = parser.GetLastError();
            // handle error code with respective descriptions
        }
    }
}

// parser automatically disposed here.


```