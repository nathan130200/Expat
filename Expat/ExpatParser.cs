using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Expat;

public sealed class ExpatParser : IDisposable
{
    readonly ExpatEncoding _encoding;
    internal GCHandle _userData;
    internal volatile bool _disposed;
    internal nint _parser;
    internal bool _strict = false;

    internal StringBuilder _cdataSection = new();
    internal volatile bool _isCdataSection = false;

    public event Action<string, IReadOnlyDictionary<string, string>> OnStartElement;
    public event Action<string> OnEndElement;
    public event Action<string> OnComment;
    public event Action<string> OnCdata;
    public event Action<string> OnText;

    private StartElementHandler _onStartElementCallback;
    private EndElementHandler _onEndElementCallback;
    private CdataSectionHandler _onCdataSectionStartCallback;
    private CdataSectionHandler _onCdataSectionEndCallback;
    private CommentHandler _onCommentCallback;
    private CharacterDataHandler _onCharacterDataCallback;
    private EntityDeclHandler _onEntityDeclCallback;

    public ExpatParser(ExpatEncoding encoding = default, bool strict = false)
    {
        _encoding = encoding ?? ExpatEncoding.UTF8;
        _strict = strict;

        _parser = Native.XML_ParserCreate(_encoding.Name);

        if (_parser == 0)
            throw new ExpatException(ExpatParserError.NoMemory);

        _userData = GCHandle.Alloc(this);

        Init();

        _onStartElementCallback = new(OnStartElementCallback);
        _onEndElementCallback = new(OnEndElementCallback);
        _onCdataSectionStartCallback = new(OnCdataSectionStartCallback);
        _onCdataSectionEndCallback = new(OnCdataSectionEndCallback);
        _onCommentCallback = new(OnCommentCallback);
        _onCharacterDataCallback = new(OnCharacterDataCallback);
    }

    void Init()
    {
        Native.XML_SetUserData(_parser, (nint)_userData);
        Native.XML_SetElementHandler(_parser, _onStartElementCallback, _onEndElementCallback);
        Native.XML_SetCdataSectionHandler(_parser, _onCdataSectionStartCallback, _onCdataSectionEndCallback);
        Native.XML_SetCommentHandler(_parser, _onCommentCallback);
        Native.XML_SetCharacterDataHandler(_parser, _onCharacterDataCallback);

        if (_strict)
        {
            _onEntityDeclCallback = new(StrictEntityDeclCallback);
            Native.XML_SetEntityDeclHandler(_parser, _onEntityDeclCallback);
        }
    }

    public bool Suspend(bool resumable = true)
    {
        var result = Native.XML_StopParser(_parser, resumable);

        if (result == ExpatParserStatus.Error)
        {
            throw new ExpatException(GetLastError())
            {
                LineNumber = LineNumber,
                LinePosition = LinePosition,
            };
        }

        return result == ExpatParserStatus.Success;
    }

    public bool Resume()
    {
        var result = Native.XML_ResumeParser(_parser);

        if (result == ExpatParserStatus.Error)
        {
            throw new ExpatException(GetLastError())
            {
                LineNumber = LineNumber,
                LinePosition = LinePosition,
            };
        }

        return result == ExpatParserStatus.Success;
    }

    public void Reset()
    {
        ThrowIfDisposed();
        Native.XML_ParserReset(_parser, _encoding.Name);
        Init();
    }

    internal void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_userData.IsAllocated)
            _userData.Free();

        _userData = default;

        if (_parser != 0)
        {
            Native.XML_ParserFree(_parser);
            _parser = 0;
        }

        _onStartElementCallback = null;
        _onEndElementCallback = null;
        _onCdataSectionStartCallback = null;
        _onCdataSectionEndCallback = null;
        _onCommentCallback = null;
        _onCharacterDataCallback = null;
        _onEntityDeclCallback = null;

        _cdataSection?.Clear();
        _cdataSection = null;
    }

    public void SetParamEntityParsing(ExpatParamEntityParsing mode)
    {
        Native.XML_SetParamEntityParsing(_parser, mode);
    }

    public int LineNumber
    {
        get
        {
            if (_disposed)
                return -1;

            return Native.XML_GetCurrentLineNumber(_parser);
        }
    }

    public int LinePosition
    {
        get
        {
            if (_disposed)
                return -1;

            return Native.XML_GetCurrentColumnNumber(_parser);
        }
    }

    internal ExpatParserError GetLastError()
    {
        if (_disposed)
            return ExpatParserError.UnexpectedState;

        return Native.XML_GetErrorCode(_parser);
    }

    public void SetBillionLaughsAttackProtection(float? maximumAmplificationFactor = default, long? activationThresholdBytes = default)
    {
        ThrowIfDisposed();

        if (maximumAmplificationFactor.HasValue)
            Native.XML_SetBillionLaughsAttackProtectionMaximumAmplification(_parser, (float)maximumAmplificationFactor);

        if (activationThresholdBytes.HasValue)
            Native.XML_SetBillionLaughsAttackProtectionActivationThreshold(_parser, (long)activationThresholdBytes);
    }

    public bool TryParse(byte[] bytes, int length, out ExpatParserError error, bool isFinalBlock = false)
    {
        error = ExpatParserError.UnexpectedState;

        if (_disposed)
            return false;

        var buf = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        try
        {
            var result = Native.XML_Parse(_parser, buf.AddrOfPinnedObject(), length, isFinalBlock);

            if (result != ExpatParserStatus.Success)
            {
                error = GetLastError();
                return false;
            }

            error = ExpatParserError.None;
            return true;
        }
        finally
        {
            buf.Free();
        }
    }

    public void Parse(byte[] bytes, int length, bool isFinalBlock = false)
    {
        ThrowIfDisposed();

        if (!TryParse(bytes, length, out var error, isFinalBlock))
        {
            throw new ExpatException(error)
            {
                LineNumber = LineNumber,
                LinePosition = LinePosition
            };
        }
    }

    #region Event Invocation

    void FireStartElement(string tagName, Dictionary<string, string> attrs)
        => OnStartElement?.Invoke(tagName, attrs);

    void FireEndElement(string tagName)
        => OnEndElement?.Invoke(tagName);

    void FireOnCdata()
    {
        var buf = _cdataSection.ToString();
        _cdataSection.Clear();
        OnCdata?.Invoke(buf);
    }

    void FireOnComment(string val)
        => OnComment?.Invoke(val);

    void FireOnText(string val)
        => OnText?.Invoke(val);

    #endregion

    #region Native Callbacks

    static void OnStartElementCallback(nint userData, nint tagName_, nint attrList_)
    {
        if (GCHandle.FromIntPtr(userData).Target is not ExpatParser parser)
            throw new InvalidOperationException();

        //tagName is null terminated string.
        var tagName = Marshal.PtrToStringAnsi(tagName_);

        var attrList = new Dictionary<string, string>();

        // Get num of attributes parsed.
        var numAttributes = Native.XML_GetSpecifiedAttributeCount(parser._parser);

        // attributes are XML_Char**, for each XML_Char* is followed by: name, value, name, value, etc.
        for (int i = 0; i < numAttributes; i += 2)
        {
            // attrName = ofs * pointerSize
            var attrName_ = Marshal.ReadIntPtr(attrList_, i * IntPtr.Size);

            // attrValue = (ofs + 1) * pointerSize
            var attrValue_ = Marshal.ReadIntPtr(attrList_, (i + 1) * IntPtr.Size);

            // read null-terminated string for both name and value
            attrList[Marshal.PtrToStringAnsi(attrName_)] = Marshal.PtrToStringAnsi(attrValue_);
        }

        parser.FireStartElement(tagName, attrList);
    }

    static void OnEndElementCallback(nint userData, nint tagName_)
    {
        if (GCHandle.FromIntPtr(userData).Target is not ExpatParser parser)
            throw new InvalidOperationException();

        // tag name is null terminated string
        var tagName = Marshal.PtrToStringAnsi(tagName_);

        parser.FireEndElement(tagName);
    }

    static void OnCdataSectionStartCallback(nint userData)
    {
        if (GCHandle.FromIntPtr(userData).Target is not ExpatParser parser)
            throw new InvalidOperationException();

        // cdata open token parsed.
        parser._isCdataSection = true;
    }

    static void OnCdataSectionEndCallback(nint userData)
    {
        if (GCHandle.FromIntPtr(userData).Target is not ExpatParser parser)
            throw new InvalidOperationException();

        // cdata close token parsed.
        parser.FireOnCdata();
        parser._isCdataSection = false;
    }

    static unsafe void OnCharacterDataCallback(nint userData, nint buf, int len)
    {
        if (GCHandle.FromIntPtr(userData).Target is not ExpatParser parser)
            throw new InvalidOperationException();

        var bytes = (byte*)buf;
        var content = parser._encoding.NativeEncoding.GetString(bytes, len);

        if (parser._isCdataSection)
            parser._cdataSection.Append(content);
        else
            parser.FireOnText(content);
    }

    static void OnCommentCallback(nint userData, nint data)
    {
        if (GCHandle.FromIntPtr(userData).Target is not ExpatParser parser)
            throw new InvalidOperationException();

        // comment is null terminated string

        parser.FireOnComment(Marshal.PtrToStringAnsi(data));
    }

    static void StrictEntityDeclCallback(nint userData,
        nint entityName_,
        int isParameterEntity,
        nint value_, int valueLength,
        nint base_,
        nint systemId_,
        nint publicId_,
        nint notationName_)
    {
        if (GCHandle.FromIntPtr(userData).Target is not ExpatParser parser)
            throw new InvalidOperationException();

        Native.XML_StopParser(parser._parser, false);
    }

    #endregion

    #region Safe Conversion

    public static implicit operator nint(ExpatParser p)
    {
        p.ThrowIfDisposed();
        return p._parser;
    }

    #endregion
}
