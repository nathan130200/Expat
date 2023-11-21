using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Expat;

public static class PInvoke
{
    const string LibName = "libexpat";

    [DllImport(LibName)]
    public static extern nint XML_ParserCreate(
        [MarshalAs(UnmanagedType.LPStr)]
        string encoding = default);

    [DllImport(LibName)]
    public static extern byte XML_ParserReset(nint parser,
        [MarshalAs(UnmanagedType.LPStr)]
        string encoding = default);

    [DllImport(LibName)]
    public static extern void XML_SetXmlDeclHandler(nint parser,
        XML_XmlDeclHandler xmldecl);

    [DllImport(LibName)]
    public static extern void XML_SetElementHandler(nint parser,
        XML_StartElementHandler start, XML_EndElementHandler end);

    [DllImport(LibName)]
    public static extern void XML_SetStartElementHandler(nint parser,
        XML_StartElementHandler start);

    [DllImport(LibName)]
    public static extern void XML_SetEndElementHandler(nint parser,
        XML_EndElementHandler end);

    [DllImport(LibName)]
    public static extern void XML_SetCharacterDataHandler(nint parser,
        XML_CharacterDataHandler handler);

    [DllImport(LibName)]
    public static extern void XML_SetProcessingInstructionHandler(nint parser,
        XML_ProcessingInstructionHandler handler);

    [DllImport(LibName)]
    public static extern void XML_SetCommentHandler(nint parser, XML_CommentHandler handler);

    [DllImport(LibName)]
    public static extern void XML_SetCdataSectionHandler(nint parser,
        XML_CdataSectionHandler start, XML_CdataSectionHandler end);

    [DllImport(LibName)]
    public static extern void XML_SetStartCdataSectionHandler(nint parser,
        XML_CdataSectionHandler handler);

    [DllImport(LibName)]
    public static extern void XML_SetEndCdataSectionHandler(nint parser,
        XML_CdataSectionHandler handler);

    [DllImport(LibName)]
    public static extern ErrorCode XML_GetErrorCode(nint parser);

    [DllImport(LibName)]
    public static extern long XML_GetCurrentLineNumber(nint parser);

    [DllImport(LibName)]
    public static extern long XML_GetCurrentColumnNumber(nint parser);

    [DllImport(LibName)]
    public static extern int XML_GetCurrentByteIndex(nint parser);

    [DllImport(LibName)]
    public static extern int XML_GetCurrentByteCount(nint parser);

    [DllImport(LibName)]
    public static extern int XML_GetSpecifiedAttributeCount(nint parser);

    [DllImport(LibName)]
    public static extern Result XML_Parse(nint parser, nint bufferPtr, int len,
        [MarshalAs(UnmanagedType.Bool)] bool isFinal);

    [DllImport(LibName)]
    public static extern Result XML_StopParser(nint parser,
        [MarshalAs(UnmanagedType.Bool)] bool resumable);

    [DllImport(LibName)]
    public static extern Result XML_ResumeParser(nint parser);

    [DllImport(LibName)]
    public static extern void XML_ParserFree(nint parser);

    [DllImport(LibName)]
    public static extern nint XML_ErrorString(ErrorCode code);

    [DllImport(LibName)]
    public static extern nint XML_ExpatVersion();

    [DllImport(LibName)]
    public static extern ExpatVersionInfo XML_ExpatVersionInfo();

    [DllImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool XML_SetBillionLaughsAttackProtectionMaximumAmplification(
        nint parser, float maximumAmplificationFactor);

    [DllImport(LibName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool XML_SetBillionLaughsAttackProtectionActivationThreshold(
        nint parser, long activationThresholdBytes);

    [DllImport(LibName)]
    public static extern int XML_SetParamEntityParsing(nint parser,
        ParamEntityParsingType type);

    [DllImport(LibName)]
    public static extern void XML_GetParsingStatus(nint parser,
        [In, Out] ref ParserStatus status);

    unsafe static PInvoke()
    {
        var ver = XML_ExpatVersionInfo();
        Debug.WriteLine($"Expat - v{ver}");
    }
}

public static class ParserExtensions
{
    public static Parser SetBillionLaughsAttackProtection(this Parser p, float? maxAmplificationFactor = default, long? thresholdBytes = default)
    {
        if (maxAmplificationFactor.HasValue)
            PInvoke.XML_SetBillionLaughsAttackProtectionMaximumAmplification(p.CPointer, maxAmplificationFactor.Value);

        if (thresholdBytes.HasValue)
            PInvoke.XML_SetBillionLaughsAttackProtectionActivationThreshold(p.CPointer, thresholdBytes.Value);

        return p;
    }
}