namespace Expat.EventArgs;

public class ElementEventArgs : ParserEventArgs
{
    public string TagName { get; init; }
    public int Depth { get; init; }
}
