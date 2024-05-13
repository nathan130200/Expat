#pragma warning disable

using System.Runtime.InteropServices;
using System.Text;
using Expat;
using Expat.Native;

namespace Expat.Native;

[StructLayout(LayoutKind.Sequential)]
public struct EXPAT_PARSER_INTERFACE
{
    public readonly nint UserData;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EXPAT_FEATURE_INTERFACE
{
    public FeatureType Type;
    public XML_Char* Name;
    public uint Value;
}

public readonly struct FeatureInfo
{
    public FeatureType Type { get; init; }
    public string Name { get; init; }
    public uint Value { get; init; }
}

public enum FeatureType
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
    const string LibraryName = "libexpat";
    const CallingConvention CallConv = CallingConvention.Cdecl;

    static Dictionary<EncodingType, (string name, Encoding provider)> s_encodingMap = new()
    {
        [EncodingType.ASCII] = ("US-ASCII", Encoding.ASCII),
        [EncodingType.UTF16] = ("UTF-16", Encoding.Unicode),
        [EncodingType.ISO88591] = ("ISO-8859-1", Encoding.Latin1),
        [EncodingType.UTF8] = ("UTF-8", Encoding.UTF8)
    };

    public static (string name, Encoding enc) GetEncoding(EncodingType type)
    {
        if (!s_encodingMap.TryGetValue(type, out var result))
            return s_encodingMap[EncodingType.UTF8];

        return result;
    }

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_ParserCreate")]
    public static extern nint ParserCreate(string? encoding);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_ParserFree")]
    public static extern void ParserFree(nint parser);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_ParserReset")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ParserReset(nint parser, string? encoding);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_Parse")]
    public static extern Status Parse(nint parser, byte* buffer,
        [MarshalAs(UnmanagedType.I4)] int length,
        [MarshalAs(UnmanagedType.I1)] bool isFinal);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_StopParser")]
    public static extern Status StopParser(nint parser, [MarshalAs(UnmanagedType.I1)] bool resumable);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_ResumeParser")]
    public static extern Status ResumeParser(nint parser);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_GetParsingStatus")]
    static extern void _GetParsingStatus(nint parser, [In, Out] ref ParsingState state);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetStartElementHandler")]
    public static extern void SetStartElementHandler(nint parser, StartElementHandler start);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetEndElementHandler")]
    public static extern void SetEndElementHandler(nint parser, StartElementHandler start);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetElementHandler")]
    public static extern void SetElementHandler(nint parser, StartElementHandler start, EndElementHandler end);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetCharacterDataHandler")]
    public static extern void SetCharacterDataHandler(nint parser, CharacterDataHandler handler);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetProcessingInstructionHandler")]
    public static extern void SetProcessingInstructionHandler(nint parser, ProcessingInstructionHandler handler);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetStartCdataSectionHandler")]
    public static extern void SetStartCdataSectionHandler(nint parser, CdataSectionHandler start);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetEndCdataSectionHandler")]
    public static extern void SetEndCdataSectionHandler(nint parser, CdataSectionHandler end);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetCdataSectionHandler")]
    public static extern void SetCdataSectionHandler(nint parser, CdataSectionHandler start, CdataSectionHandler end);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetCommentHandler")]
    public static extern void SetCommentHandler(nint parser, CommentHandler handler);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetXmlDeclHandler")]
    public static extern void SetPrologHandler(nint parser, PrologHandler handler);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetParamEntityParsing")]
    public static extern void SetParamEntityParsing(nint parser, ParamEntityParsingType type);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_UseForeignDTD")]
    public static extern Error UseForeignDTD(nint parser, [MarshalAs(UnmanagedType.I1)] bool useDTD);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_GetFeatureList")]
    static extern EXPAT_FEATURE_INTERFACE* _GetFeatureList();

    public unsafe static IReadOnlyList<FeatureInfo> GetFeatureList()
    {
        var result = new List<FeatureInfo>();
        var ptr = (EXPAT_FEATURE_INTERFACE*)_GetFeatureList();

        while (ptr->Type != FeatureType.XML_FEATURE_END)
        {
            result.Add(new FeatureInfo
            {
                Type = ptr->Type,
                Name = new string(ptr->Name),
                Value = ptr->Value
            });

            ptr++;
        }

        return result.AsReadOnly();
    }

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_GetErrorCode")]
    public static extern Error GetErrorCode(nint parser);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_ErrorString")]
    static extern XML_Char* _GetErrorString(Error error);

    public static string GetErrorString(Error error)
        => new string(_GetErrorString(error));

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_ExpatVersion", CharSet = CharSet.Ansi)]
    static extern XML_Char* _GetExpatVersionString();

    public static string GetExpatVersionString(out string version)
        => version = new string(_GetExpatVersionString());

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_ExpatVersionInfo")]
    public static extern VersionInfo GetExpatVersion();

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_GetCurrentByteIndex")]
    public static extern int GetCurrentByteIndex(nint parser);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_GetCurrentByteCount")]
    public static extern int GetCurrentByteCount(nint parser);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_GetCurrentLineNumber")]
    public static extern long GetCurrentLineNumber(nint parser);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_GetCurrentColumnNumber")]
    public static extern long GetCurrentColumnNumber(nint parser);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetBillionLaughsAttackProtectionMaximumAmplification")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool SetBillionLaughsAttackProtectionMaximumAmplification(nint parser, float maximumAmplificationFactor);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetBillionLaughsAttackProtectionActivationThreshold")]
    public static extern bool SetBillionLaughsAttackProtectionActivationThreshold(nint parser, long activationThresholdBytes);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetUserData")]
    public static extern void SetUserData(nint parser, nint userData);

    public static unsafe nint GetUserData(nint parser)
    {
        return ((EXPAT_PARSER_INTERFACE*)parser)->UserData;
    }

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_UseParserAsHandlerArg")]
    public static extern void UseParserAsHandlerArg(nint parser);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_GetSpecifiedAttributeCount")]
    public static extern int GetSpecifiedAttributeCount(nint parser);

    [DllImport(LibraryName, CallingConvention = CallConv, EntryPoint = "XML_SetEncoding")]
    public static extern Status SetEncoding(nint parser);

    public static ParsingState GetParsingStatus(nint parser)
    {
        ParsingState result = default;
        _GetParsingStatus(parser, ref result);
        return result;
    }
}

#pragma warning restore