using System.Runtime.InteropServices;

namespace Expat;

public static class Native
{
    const string LibraryName = "libexpat";

    [DllImport(LibraryName)]
    public static extern nint XML_ParserCreate(string encoding);

    [DllImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool XML_ParserReset(nint parser, string encoding);

    [DllImport(LibraryName)]
    public static extern void XML_ParserFree(nint parser);

    [DllImport(LibraryName)]
    public static extern ExpatParserStatus XML_SetEncoding(nint parser, string encoding);

    [DllImport(LibraryName)]
    public static extern ExpatParserError XML_GetErrorCode(nint parser);

    [DllImport(LibraryName)]
    public static extern void XML_SetElementHandler(nint parser, StartElementHandler start, EndElementHandler end);

    [DllImport(LibraryName)]
    public static extern void XML_SetCharacterDataHandler(nint parser, CharacterDataHandler handler);

    [DllImport(LibraryName)]
    public static extern void XML_SetCommentHandler(nint parser, CommentHandler handler);

    [DllImport(LibraryName)]
    public static extern bool XML_SetParamEntityParsing(nint parser, ExpatParamEntityParsing mode);

    [DllImport(LibraryName)]
    public static extern void XML_SetCdataSectionHandler(nint parser, CdataSectionHandler start, CdataSectionHandler end);

    [DllImport(LibraryName)]
    public static extern void XML_SetEntityDeclHandler(nint parser, EntityDeclHandler handler);

    [DllImport(LibraryName)]
    public static extern ExpatParserStatus XML_StopParser(nint parser, bool resumable);

    [DllImport(LibraryName)]
    public static extern ExpatParserStatus XML_ResumeParser(nint parser);

    [DllImport(LibraryName)]
    public static extern int XML_GetSpecifiedAttributeCount(nint parser);

    [DllImport(LibraryName)]
    public static extern ExpatParserStatus XML_Parse(nint parser, nint s, int len, [MarshalAs(UnmanagedType.I1)] bool isFinal);

    [DllImport(LibraryName)]
    public static extern int XML_GetCurrentLineNumber(nint parser);

    [DllImport(LibraryName)]
    public static extern int XML_GetCurrentColumnNumber(nint parser);

    [DllImport(LibraryName)]
    public static extern nint XML_ErrorString(ExpatParserError code);

    [DllImport(LibraryName)]
    public static extern bool XML_SetBillionLaughsAttackProtectionMaximumAmplification(nint parser, float maximumAmplificationFactor);

    [DllImport(LibraryName)]
    public static extern bool XML_SetBillionLaughsAttackProtectionActivationThreshold(nint parser, long activationThresholdBytes);

    [DllImport(LibraryName)]
    public static extern void XML_SetUserData(nint parser, nint userData);
}

public enum ExpatParamEntityParsing
{
    Never,
    UnlessStandalone,
    Always
}