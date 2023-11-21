namespace Expat.EventArgs;

public sealed class ElementEventArgs : ParserEventArgs
{
    public string Name { get; }
    public IReadOnlyDictionary<string, string> Attributes { get; }

    public ElementEventArgs(Parser parser, string name, IReadOnlyDictionary<string, string> attrs) : base(parser)
    {
        Name = name;
        Attributes = attrs;
    }
}
