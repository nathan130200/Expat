using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Expat;

public static class PInvoke
{
    const string LibPath = "libexpat";

    [DllImport(LibPath)]
    public static extern nint XML_ParserCreate(nint encoding = default);

    [DllImport(LibPath)]
    public static extern byte XML_ParserReset(nint parser, nint encoding = default);

    [DllImport(LibPath)]
    public static extern void XML_SetXmlDeclHandler(nint parser, XML_XmlDeclHandler xmldecl);

    [DllImport(LibPath)]
    public static extern void XML_SetElementHandler(nint parser, XML_StartElementHandler start, XML_EndElementHandler end);

    [DllImport(LibPath)]
    public static extern void XML_SetStartElementHandler(nint parser, XML_StartElementHandler start);

    [DllImport(LibPath)]
    public static extern void XML_SetEndElementHandler(nint parser, XML_EndElementHandler end);

    [DllImport(LibPath)]
    public static extern void XML_SetCharacterDataHandler(nint parser, XML_CharacterDataHandler handler);

    [DllImport(LibPath)]
    public static extern void XML_SetProcessingInstructionHandler(nint parser, XML_ProcessingInstructionHandler handler);

    [DllImport(LibPath)]
    public static extern void XML_SetCommentHandler(nint parser, XML_CommentHandler handler);

    [DllImport(LibPath)]
    public static extern void XML_SetCdataSectionHandler(nint parser, XML_CdataSectionHandler start, XML_CdataSectionHandler end);

    [DllImport(LibPath)]
    public static extern void XML_SetStartCdataSectionHandler(nint parser, XML_CdataSectionHandler handler);

    [DllImport(LibPath)]
    public static extern void XML_SetEndCdataSectionHandler(nint parser, XML_CdataSectionHandler handler);

    [DllImport(LibPath)]
    public static extern XmlError XML_GetErrorCode(nint parser);

    [DllImport(LibPath)]
    public static extern long XML_GetCurrentLineNumber(nint parser);

    [DllImport(LibPath)]
    public static extern long XML_GetCurrentColumnNumber(nint parser);

    [DllImport(LibPath)]
    public static extern int XML_GetCurrentByteIndex(nint parser);

    [DllImport(LibPath)]
    public static extern int XML_GetCurrentByteCount(nint parser);

    [DllImport(LibPath)]
    public static extern int XML_GetSpecifiedAttributeCount(nint parser);

    [DllImport(LibPath)]
    public static extern XmlStatus XML_Parse(nint parser, nint bufferPtr, int len,
        [MarshalAs(UnmanagedType.Bool)] bool isFinal);

    [DllImport(LibPath)]
    public static extern XmlStatus XML_StopParser(nint parser,
        [MarshalAs(UnmanagedType.Bool)] bool resumable);

    [DllImport(LibPath)]
    public static extern XmlStatus XML_ResumeParser(nint parser);

    [DllImport(LibPath)]
    public static extern void XML_ParserFree(nint parser);

    [DllImport(LibPath)]
    public static extern nint XML_ErrorString(XmlError code);

    [DllImport(LibPath)]
    public static extern nint XML_ExpatVersion();

    static PInvoke()
    {
        Debug.WriteLine($"Using libexpat v{XML_ExpatVersion()}");
    }
}
