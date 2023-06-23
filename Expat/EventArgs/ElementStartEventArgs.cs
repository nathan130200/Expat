namespace Expat.EventArgs;

public class ElementStartEventArgs : ElementEventArgs
{
    public IReadOnlyDictionary<string, string> Attributes { get; init; }
}
