namespace Expat.EventArgs;

public sealed class TextEventArgs : ParserEventArgs
{
    public TextEventArgs(Parser parser, string value) : base(parser)
    {
        Value = value;
    }

    public string Value { get; }
}
