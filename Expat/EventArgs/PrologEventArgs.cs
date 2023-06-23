namespace Expat.EventArgs;

public sealed class PrologEventArgs : ParserEventArgs
{
    public string Version { get; init; }
    public string Encoding { get; init; }
    public bool? Standalone { get; init; }
}
