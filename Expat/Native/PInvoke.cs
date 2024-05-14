#pragma warning disable

using System.Runtime.InteropServices;
using System.Text;
using Expat;
using Expat.Native;

namespace Expat.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct EXPAT_PARSER_INTERFACE
{
    internal readonly nint UserData;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct EXPAT_FEATURE_INTERFACE
{
    internal FeatureType Type;
    internal XML_Char* Name;
    internal uint Value;
}

internal enum FeatureType
{
    XML_FEATURE_END = 0,
    XML_FEATURE_UNICODE,
    XML_FEATURE_UNICODE_WCHAR_T,
    XML_FEATURE_DTD,
    XML_FEATURE_CONTEXT_BYTES,
    XML_FEATURE_MIN_SIZE,
    XML_FEATURE_SIZEOF_XML_CHAR,
    XML_FEATURE_SIZEOF_XML_LCHAR,
    XML_FEATURE_NS,
    XML_FEATURE_LARGE_SIZE,
    XML_FEATURE_ATTR_INFO,
    XML_FEATURE_BILLION_LAUGHS_ATTACK_PROTECTION_MAXIMUM_AMPLIFICATION_DEFAULT,
    XML_FEATURE_BILLION_LAUGHS_ATTACK_PROTECTION_ACTIVATION_THRESHOLD_DEFAULT,
    XML_FEATURE_GE
}

public unsafe static class PInvoke
{
    const string s_LibraryName = "libexpat";
    const CallingConvention s_CallConv = CallingConvention.Cdecl;

    static Dictionary<EncodingType, (string name, Encoding provider)> s_encodingMap = new()
    {
        [EncodingType.ASCII] = ("US-ASCII", Encoding.ASCII),
        [EncodingType.UTF16] = ("UTF-16", Encoding.Unicode),
        [EncodingType.ISO88591] = ("ISO-8859-1", Encoding.Latin1),
        [EncodingType.UTF8] = ("UTF-8", Encoding.UTF8)
    };

    public static (string name, Encoding enc) GetEncoding(this EncodingType type)
    {
        if (!s_encodingMap.TryGetValue(type, out var result))
            return s_encodingMap[EncodingType.UTF8];

        return result;
    }

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_ParserCreate")]
    internal static extern nint ParserCreate(string? encoding);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_ParserFree")]
    internal static extern void ParserFree(nint parser);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_ParserReset")]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ParserReset(nint parser, string? encoding);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_Parse")]
    internal static extern Status Parse(nint parser, byte* buffer,
        [MarshalAs(UnmanagedType.I4)] int length,
        [MarshalAs(UnmanagedType.I1)] bool isFinal);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_StopParser")]
    internal static extern Status StopParser(nint parser, [MarshalAs(UnmanagedType.I1)] bool resumable);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_ResumeParser")]
    internal static extern Status ResumeParser(nint parser);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_GetParsingStatus")]
    static extern void _GetParsingStatus(nint parser, [In, Out] ref ParsingState state);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetStartElementHandler")]
    internal static extern void SetStartElementHandler(nint parser, StartElementHandler start);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetEndElementHandler")]
    internal static extern void SetEndElementHandler(nint parser, StartElementHandler start);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetElementHandler")]
    internal static extern void SetElementHandler(nint parser, StartElementHandler start, EndElementHandler end);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetCharacterDataHandler")]
    internal static extern void SetCharacterDataHandler(nint parser, CharacterDataHandler handler);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetProcessingInstructionHandler")]
    internal static extern void SetProcessingInstructionHandler(nint parser, ProcessingInstructionHandler handler);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetStartCdataSectionHandler")]
    internal static extern void SetStartCdataSectionHandler(nint parser, CdataSectionHandler start);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetEndCdataSectionHandler")]
    internal static extern void SetEndCdataSectionHandler(nint parser, CdataSectionHandler end);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetCdataSectionHandler")]
    internal static extern void SetCdataSectionHandler(nint parser, CdataSectionHandler start, CdataSectionHandler end);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetCommentHandler")]
    internal static extern void SetCommentHandler(nint parser, CommentHandler handler);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetXmlDeclHandler")]
    internal static extern void SetPrologHandler(nint parser, PrologHandler handler);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_GetFeatureList")]
    static extern EXPAT_FEATURE_INTERFACE* _GetFeatureList();

    internal unsafe static IReadOnlyList<ExpatFeatureInfo> GetFeatureList()
    {
        var result = new List<ExpatFeatureInfo>();
        var ptr = (EXPAT_FEATURE_INTERFACE*)_GetFeatureList();

        while (ptr->Type != FeatureType.XML_FEATURE_END)
        {
            result.Add(new ExpatFeatureInfo
            {
                Type = ptr->Type,
                Name = new string(ptr->Name),
                Value = ptr->Value
            });

            ptr++;
        }

        return result.AsReadOnly();
    }

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_GetErrorCode")]
    internal static extern Error GetErrorCode(nint parser);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_ErrorString")]
    static extern XML_Char* _GetErrorString(Error error);

    internal static string GetErrorString(Error error)
        => new string(_GetErrorString(error));

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_ExpatVersion", CharSet = CharSet.Ansi)]
    static extern XML_Char* _GetExpatVersionString();

    internal static string GetExpatVersionString(out string version)
        => version = new string(_GetExpatVersionString());

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_ExpatVersionInfo")]
    internal static extern VersionInfo GetExpatVersion();

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_GetCurrentByteIndex")]
    internal static extern int GetCurrentByteIndex(nint parser);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_GetCurrentByteCount")]
    internal static extern int GetCurrentByteCount(nint parser);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_GetCurrentLineNumber")]
    internal static extern long GetCurrentLineNumber(nint parser);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_GetCurrentColumnNumber")]
    internal static extern long GetCurrentColumnNumber(nint parser);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetBillionLaughsAttackProtectionMaximumAmplification")]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool SetBillionLaughsAttackProtectionMaximumAmplification(nint parser, float maximumAmplificationFactor);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetBillionLaughsAttackProtectionActivationThreshold")]
    internal static extern bool SetBillionLaughsAttackProtectionActivationThreshold(nint parser, long activationThresholdBytes);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetUserData")]
    internal static extern void SetUserData(nint parser, nint userData);

    internal static unsafe nint GetUserData(nint parser)
    {
        return ((EXPAT_PARSER_INTERFACE*)parser)->UserData;
    }

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_UseParserAsHandlerArg")]
    internal static extern void UseParserAsHandlerArg(nint parser);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_GetSpecifiedAttributeCount")]
    internal static extern int GetSpecifiedAttributeCount(nint parser);

    [DllImport(s_LibraryName, CallingConvention = s_CallConv, EntryPoint = "XML_SetEncoding")]
    internal static extern Status SetEncoding(nint parser, string? encoding);

    internal static ParsingState GetParsingStatus(nint parser)
    {
        ParsingState result = default;
        _GetParsingStatus(parser, ref result);
        return result;
    }
}

#pragma warning restore