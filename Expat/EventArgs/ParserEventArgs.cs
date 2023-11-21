namespace Expat.EventArgs;

public abstract class ParserEventArgs : System.EventArgs
{
    public Parser Parser { get; internal init; }

    public ParserEventArgs(Parser parser)
    {
        Parser = parser;
        CurrentLineNumber = Parser.CurrentLineNumber;
        CurrentColumnNumber = Parser.CurrentColumnNumber;
        CurrentByteIndex = Parser.CurrentByteIndex;
        CurrentByteCount = Parser.CurrentByteCount;
    }

    public long CurrentLineNumber { get; }
    public long CurrentColumnNumber { get; }
    public int CurrentByteIndex { get; }
    public int CurrentByteCount { get; }
}
