namespace Expat.EventArgs;

public class ProcessingInstructionEventArgs : ParserEventArgs
{
    public string Target { get; init; }
    public string Data { get; init; }
}
