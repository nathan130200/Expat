using System;

namespace Expat;

public class ExpatException : Exception
{
    public ExpatParserError Code { get; }

    internal ExpatException()
    {
        Code = ExpatParserError.None;
    }

    public int LineNumber { get; init; }
    public int LinePosition { get; init; }

    internal ExpatException(ExpatParserError error) : base(error.GetMessage())
    {
        Code = error;
    }

    internal ExpatException(ExpatParserError error, string message) : base(message)
    {
        Code = error;
    }
}