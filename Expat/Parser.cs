namespace Expat;

using Expat.EventArgs;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static Expat.PInvoke;

internal record class ExpatEncoding(ExpatEncodingType Type, string Name, Encoding Encoding)
{
    public string LocalName => Encoding?.EncodingName;

    static readonly List<ExpatEncoding> s_cache = new()
    {
        new(ExpatEncodingType.Utf8, "UTF-8", Encoding.UTF8),
        new(ExpatEncodingType.Ascii, "US-ASCII", Encoding.ASCII),
        new(ExpatEncodingType.Latin1, "ISO-8859-1", Encoding.Latin1),
        new(ExpatEncodingType.Utf16, "UTF-16LE", Encoding.Unicode),
        new(ExpatEncodingType.Utf16LittleEndian, "UTF-16LE", Encoding.Unicode),
        new(ExpatEncodingType.Utf16BigEndian, "UTF-16BE", Encoding.BigEndianUnicode),
    };

    public static ExpatEncoding FromType(ExpatEncodingType type)
    {
        foreach (var it in s_cache)
        {
            if (it.Type == type)
                return it;
        }

        throw new NotSupportedException($"Encoding is not supported '{type}'");
    }

    public static ExpatEncoding FromName(string name)
    {
        foreach (var it in s_cache)
        {
            if (it.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return it;

            if (it.Encoding.EncodingName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return it;
        }

        return s_cache[0]; // fallback to UTF-8
    }
}

public class Parser : IDisposable
{
    private ExpatEncoding _encoding;
    private volatile bool _disposed;
    private volatile bool _isCdata;
    private volatile int _depth;
    private nint _handle;

    private XML_StartElementHandler _onElementStart;
    private XML_EndElementHandler _onElementEnd;
    private XML_CdataSectionHandler _onCdataStart;
    private XML_CdataSectionHandler _onCdataEnd;
    private XML_CommentHandler _onComment;
    private XML_CharacterDataHandler _onCharacterData;
    private XML_ProcessingInstructionHandler _onProcessingInstruction;
    private XML_XmlDeclHandler _onProlog;
    private StringBuilder _cdataBuffer;

    public event ParserEventHandler<ProcessingInstructionEventArgs> OnProcessingInstruction;
    public event ParserEventHandler<ElementEventArgs> OnElementStart;
    public event ParserEventHandler<TextEventArgs> OnElementEnd;
    public event ParserEventHandler<TextEventArgs> OnComment;
    public event ParserEventHandler<TextEventArgs> OnCdata;
    public event ParserEventHandler<TextEventArgs> OnText;
    public event ParserEventHandler<PrologEventArgs> OnProlog;



    public nint CPointer
        => _handle;

    public Parser(ExpatEncodingType? encoding = default)
    {
        if (encoding.HasValue)
            _encoding = ExpatEncoding.FromType(encoding.Value);

        _handle = XML_ParserCreate(_encoding?.Name);

        if (_handle == nint.Zero)
            throw new InvalidOperationException("Unable to create native expat parser instance.");

        _cdataBuffer = new StringBuilder();
        _onElementStart = new(OnElementStartCallback);
        _onElementEnd = new(OnElementEndCallback);
        _onCdataStart = new(OnCdataStartCallback);
        _onCdataEnd = new(OnCdataEndCallback);
        _onComment = new(OnCommentCallback);
        _onCharacterData = new(OnCharacterDataCallback);
        _onProcessingInstruction = new(OnProcessingInstructionCallback);
        _onProlog = new(OnPrologCallback);

        PostInit();
    }

    internal void PostInit()
    {
        // ensure not disposed first!
        ThrowIfDisposed();

        // bind (or rebind) callbacks to parser instance.
        XML_SetElementHandler(_handle, _onElementStart, _onElementEnd);
        XML_SetCdataSectionHandler(_handle, _onCdataStart, _onCdataEnd);
        XML_SetCharacterDataHandler(_handle, _onCharacterData);
        XML_SetProcessingInstructionHandler(_handle, _onProcessingInstruction);
        XML_SetCommentHandler(_handle, _onComment);
        XML_SetXmlDeclHandler(_handle, _onProlog);
    }

    /// <summary>
    /// Return information about the current parse location: <b>Line Number</b>
    /// </summary>
    /// <remarks>Returns <c>0</c> to indicate an error.</remarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public long CurrentLineNumber
    {
        get
        {
            ThrowIfDisposed();
            return XML_GetCurrentLineNumber(_handle);
        }
    }

    /// <summary>
    /// Return information about the current parse location: <b>Column Number</b>
    /// </summary>
    /// <remarks>Returns <c>0</c> to indicate an error.</remarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public long CurrentColumnNumber
    {
        get
        {
            ThrowIfDisposed();
            return XML_GetCurrentColumnNumber(_handle);
        }
    }

    /// <summary>
    /// Return information about the current parse location: <b>Byte Index</b>
    /// </summary>
    /// <remarks>Returns <c>-1</c> to indicate an error.</remarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int CurrentByteIndex
    {
        get
        {
            ThrowIfDisposed();
            return XML_GetCurrentByteIndex(_handle);
        }
    }

    /// <summary>
    /// Return the number of bytes in the current event.
    /// </summary>
    /// <remarks>Returns <c>0</c> if the event is in an internal entity.</remarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int CurrentByteCount
    {
        get
        {
            ThrowIfDisposed();
            return XML_GetCurrentByteCount(_handle);
        }
    }

    /// <summary>
    /// If <see cref="XML_Parse"/> have returned <see cref="Result.Error"/> then <see cref="XML_GetErrorCode"/> returns information about the error.
    /// </summary>
    public ErrorCode GetLastError()
    {
        ThrowIfDisposed();
        return XML_GetErrorCode(_handle);
    }

    public void Suspend(bool allowResume = true)
    {
        ThrowIfDisposed();

        if (XML_StopParser(_handle, allowResume) == Result.Error)
            throw new ExpatException(GetLastError());
    }

    public void Resume()
    {
        ThrowIfDisposed();

        if (XML_ResumeParser(_handle) == Result.Error)
            throw new ExpatException(GetLastError());
    }

    public void Feed(byte[] buffer, int length, out Result result, bool isFinal = false)
    {
        ThrowIfDisposed();

        Unsafe.SkipInit(out GCHandle ptr);

        lock (this)
        {
            try
            {
                ptr = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                result = XML_Parse(_handle, ptr.AddrOfPinnedObject(), length, isFinal);
            }
            finally
            {
                if (ptr.IsAllocated)
                    ptr.Free();
            }
        }
    }

    public void Feed(byte[] buffer, int length, bool isFinal = false)
    {
        Feed(buffer, length, out var result, isFinal);

        if (result != Result.Success)
            throw new ExpatException(GetLastError());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Reset()
    {
        ThrowIfDisposed();

        if (XML_ParserReset(_handle, _encoding?.Name) == 0)
            throw new ExpatException(GetLastError());

        PostInit();
    }

    public void Dispose()
    {
        ThrowIfDisposed();
        _disposed = true;
        _encoding = default;

        if (_handle != nint.Zero)
        {
            XML_ParserFree(_handle);
            _handle = nint.Zero;
        }

        _onElementStart = null;
        _onElementEnd = null;
        _onCdataStart = null;
        _onCdataEnd = null;
        _onComment = null;
        _onCharacterData = null;
        _onProcessingInstruction = null;

        _cdataBuffer?.Clear();
        _cdataBuffer = null;

        GC.SuppressFinalize(this);
    }

    protected void OnElementStartCallback(nint _, nint m_name, nint m_attributeList)
    {
        var tagName = Marshal.PtrToStringAnsi(m_name);

        var numAttributes = XML_GetSpecifiedAttributeCount(_handle);
        var strArray = new string[numAttributes];

        nint ptr;
        int index = 0;

        while ((ptr = Marshal.ReadIntPtr(m_attributeList, index * nint.Size)) != 0
            && index < strArray.Length) // ensure we will read ONLY desired values
        {
            strArray[index] = Marshal.PtrToStringAnsi(ptr);
            index++;
        }

        var attrs = new Dictionary<string, string>();

        for (int i = 0; i < strArray.Length; i += 2)
            attrs[strArray[i]] = strArray[i + 1];

        OnElementStart?.Invoke(e: new(this, tagName, attrs));
    }

    protected void OnElementEndCallback(nint _, nint m_ptr)
    {
        var tagName = Marshal.PtrToStringAnsi(m_ptr);
        OnElementEnd?.Invoke(new(this, tagName));
    }

    protected void OnCommentCallback(nint _, nint m_ptr)
    {
        var str = Marshal.PtrToStringAnsi(m_ptr);
        OnComment?.Invoke(new(this, str));
    }

    protected unsafe void OnCharacterDataCallback(nint _, nint m_ptr, int m_len)
    {
        // FIXME: Character data is in same encoding used in parser creation?
        // I should check and test this later to ensure correct bytes are received.
        // For now fallback to UTF-8 instead.

        var buf = new byte[m_len];
        Marshal.Copy(m_ptr, buf, 0, m_len);
        var str = (_encoding?.Encoding ?? Encoding.UTF8).GetString(buf); // internal encoding is utf-8
        buf = default;

        if (_isCdata)
            _cdataBuffer.Append(str);
        else
            OnText?.Invoke(new(this, str));
    }

    protected void OnProcessingInstructionCallback(nint _, nint m_target, nint m_data)
    {
        var target = Marshal.PtrToStringAnsi(m_target);
        var data = Marshal.PtrToStringAnsi(m_data);
        OnProcessingInstruction?.Invoke(new(this, target, data));
    }

    protected void OnCdataStartCallback(nint _)
    {
        _isCdata = true;
    }

    protected void OnCdataEndCallback(nint _)
    {
        var content = _cdataBuffer.ToString();
        OnCdata?.Invoke(new(this, content));
        _cdataBuffer.Clear();
        _isCdata = false;
    }

    protected void OnPrologCallback(nint _, nint m_version, nint m_encoding, int m_standalone)
    {
        var version = Marshal.PtrToStringAnsi(m_version);
        var encoding = Marshal.PtrToStringAnsi(m_encoding);

        var standalone = (Standalone)m_standalone;

        if (!Enum.IsDefined(standalone))
            standalone = Standalone.Unspecified;

        var enc = ExpatEncoding.FromName(encoding ?? "UTF-8");
        OnProlog?.Invoke(new(this, version, enc.Encoding, standalone));
        _encoding ??= enc;
    }
}
