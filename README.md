# Expat
[Expat](https://github.com/libexpat/libexpat) bindings for .NET/C#. 

## About
Expat, a C99 library for parsing [XML 1.0 Fourth Edition](https://www.w3.org/TR/2006/REC-xml-20060816/), started by [James Clark](https://en.wikipedia.org/wiki/James_Clark_%28programmer%29) in 1997. Expat is a stream-oriented XML parser. This means that you register handlers with the parser before starting the parse. These handlers are called when the parser discovers the associated structures in the document being parsed from.

## Bindings
These bindings are intended to provide most of the features available from libexpat to .NET

I even implemented a simple parser that is available for instant use if you want to read XML.

Everything needed for a simple XML parser has been publicly exposed in the code.

You might ask yourself <i>"Doesn't .NET already have something similar to read XML like `System.Xml.XmlReader`"?</i>

Exactly my friend! But I'm working on an XMPP server where it sends XML "in chunks" and this is something the `XmlReader` can't handle, as it has difficulties understanding the context of the parsing and interpreting it coherently.

Thinking about it, instead of creating dozens of work-arounds using `System.Xml.XmlReader` i ended up choosing to _"join the useful to the pleasant"_ (a very common expression here in Brazil, which in short means that we need something that works and that "something" already exists, so why not use it?).

## Basic Parser

This bindings already has a simple parser implemented by default, which basically will redirect all of the P/Invoking callbacks and convert in a simple and practical way and will fire the events as needed.

Most functions are already available. In my case, where I'll have to implement an XMPP server, I'll just need to transport this data from the events that will be triggered by the Parser and build a `System.Xml.Linq.XElement` from that, which makes everything a LOT easier.

So basically we have an example of parser in `Expat.Test` folder you can check in sample parser usage.

```cs

using Expat;

// ...

using(var parser = new Parser())
{
	parser.OnElementStart += (s, e) => 
	{
		Console.WriteLine("Start Element: " + e.TagName);

		foreach(var (name, value) in e.Attributes)
			Console.WriteLine("Attribute: {0} -> {1}", name, value);
	};

	parser.OnElementEnd += (s, e) => 
	{
		Console.WriteLine("End Element: " + e.TagName);
	};

	// do stuff with parser.
	byte[] myBuffer = "<foo><bar/></foo>"u8;
	parser.Update(myBuffer, myBuffer.Length);

	// if you will reuse parser later, don't dispose yet!
	parser.Reset();
}

// automatically releases the parser after exit from using statement block (aka goes out of scope).

```

## Notes
- As we use P/Invoking, when you create the `Parser` instance after the end of use, you MUST call `Dispose` (or use the `using statements`) to free memory and avoid any leaks!
- The `Parser` instance may be reusable as long as you **ONLY** and **EXCLUSIVELY** allow one use at a time! (eg: assigns and that instance becomes "borrowed" and cannot be used elsewhere)
- As recommended by libexpat itself call `Parser.Reset` whenever necessary to reuse the parser and also free memory. In the original `XML_ParserReset` it removes all handlers but in the builtin parser implemented in this bindings library, it automatically rebinds the callbacks to reuse the same parser instance.
- As stated earlier there is no guarantee of multithreading support, ideal would be to use semaphore mechanisms or locks to ensure parser integrity and avoid unexpected crashes from corrupted memory and other related p/invoking errors!
- Use Try/Catch to handle expat errors. Every expat function that fails will throw an `ExpatException` with respective error code and description about the error.
 
## How to get libexpat?

The easiest and simplest way is to use [VCPKG](https://github.com/microsoft/vcpkg) (C/C++ package manager for acquiring and managing libraries).

Just open a terminal (with VCPKG already installed and configured) and type:
- `vcpkg install expat` it will download, compile and install the library.

After that, just get the library from the VCPKG folder or add the vcpkg folder to your `%PATH%` and the library will load without problems. (Already tested and working with Windows x86 and x64)

There are no secrets at this point. 

- Also you can clone the repository from [libexpat](https://github.com/libexpat/libexpat) and compile manually to get the binaries. 

- Even better, there are already pre compiled releases ready for use! [https://github.com/libexpat/libexpat/releases/](https://github.com/libexpat/libexpat/releases/)

### Warning

Keep in mind about "Any Cpu", "x86" and "x64" architectures. In .NET they will impact on P/Invoker. If you try to load 64bit libexpat in a 32bit process it will fail and crash! And about "Any Cpu" architecture:

> "Any CPU", means that the assembly will run natively on the CPU it is currently running on. Meaning, it will run as 64-bit on a 64-bit machine and 32-bit on a 32-bit machine. If the assembly is called from a 64-bit application, it will perform as a 64-bit assembly and so on.

<b>TL;DR</b>: If you compile with Any CPU on a 32bit machine it will have 32bit code and P/Invoker will need to load the 32bit version of libexpat. It will depend on your processor!


### Benchmark

```ini

BenchmarkDotNet=v0.13.5, OS=Windows 10 (10.0.19045.3086/22H2/2022Update)
Intel Core i5-4210U CPU 1.70GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET SDK=7.0.304
  [Host]   : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
  ShortRun : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
|             Method |      Mean |     Error |   StdDev |   Gen0 | Allocated |
|------------------- |----------:|----------:|---------:|-------:|----------:|
|    ParseUsingExpat | 274.50 μs | 71.947 μs | 3.944 μs | 4.3945 |    7.2 KB |
| ParseUsingXElement |  13.08 μs |  7.598 μs | 0.416 μs | 7.1259 |  10.94 KB |

