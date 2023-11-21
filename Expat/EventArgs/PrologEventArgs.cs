using System.Text;

namespace Expat.EventArgs;

public sealed class PrologEventArgs : ParserEventArgs
{
    public PrologEventArgs(Parser parser, string version, Encoding encoding, Standalone standalone) : base(parser)
    {
        Version = version;
        Encoding = encoding;
        Standalone = standalone;
    }

    public string Version { get; }
    public Encoding Encoding { get; }
    public Standalone Standalone { get; }
}
