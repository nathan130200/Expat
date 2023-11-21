namespace Expat.EventArgs;

public sealed class ProcessingInstructionEventArgs : ParserEventArgs
{
    public ProcessingInstructionEventArgs(Parser parser, string target, string data) : base(parser)
    {
        Target = target;
        Data = data;
    }

    public string Target { get; init; }
    public string Data { get; init; }
}
