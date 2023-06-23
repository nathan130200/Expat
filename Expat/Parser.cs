namespace Expat;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Expat.EventArgs;
using static Expat.PInvoke;

public class Parser : IDisposable
{
    private nint _encoding = default;
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
    private StringBuilder _content;

    public event ParserEventHandler<ProcessingInstructionEventArgs> OnProcessingInstruction;
    public event ParserEventHandler<ElementStartEventArgs> OnElementStart;
    public event ParserEventHandler<ElementEventArgs> OnElementEnd;
    public event ParserEventHandler<TextEventArgs> OnComment;
    public event ParserEventHandler<TextEventArgs> OnCdata;
    public event ParserEventHandler<TextEventArgs> OnText;
    public event ParserEventHandler<PrologEventArgs> OnProlog;

    public Parser(string encoding = default)
    {
        if (!string.IsNullOrEmpty(encoding))
        {
            var buf = Encoding.UTF8.GetBytes(encoding);
            _encoding = Marshal.AllocHGlobal(buf.Length);
            Marshal.Copy(buf, 0, _encoding, buf.Length);
        }

        _content = new StringBuilder();
        _onElementStart = new(OnElementStartCallback);
        _onElementEnd = new(OnElementEndCallback);
        _onCdataStart = new(OnCdataStartCallback);
        _onCdataEnd = new(OnCdataEndCallback);
        _onComment = new(OnCommentCallback);
        _onCharacterData = new(OnCharacterDataCallback);
        _onProcessingInstruction = new(OnProcessingInstructionCallback);
        _onProlog = new(OnPrologCallback);

        _handle = XML_ParserCreate(_encoding);
        BindParserEvents();
    }

    protected void BindParserEvents()
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
    public int CurrentByteCount
    {
        get
        {
            ThrowIfDisposed();
            return XML_GetCurrentByteCount(_handle);
        }
    }

    /// <summary>
    /// If <see cref="XML_Parse"/> have returned <see cref="XmlStatus.Error"/> then <see cref="XML_GetErrorCode"/> returns information about the error.
    /// </summary>
    public XmlError GetLastError()
    {
        ThrowIfDisposed();
        return XML_GetErrorCode(_handle);
    }

    public void Suspend(bool isResumable = true)
    {
        ThrowIfDisposed();
        var status = XML_StopParser(_handle, isResumable);

        if (status == XmlStatus.Error)
            throw new ExpatException(GetLastError());
    }

    public void Resume()
    {
        ThrowIfDisposed();

        if (XML_ResumeParser(_handle) == XmlStatus.Error)
            throw new ExpatException(GetLastError());
    }

    public void Update(byte[] buffer, int length)
    {
        ThrowIfDisposed();
        Unsafe.SkipInit(out GCHandle mmm);

        try
        {
            mmm = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var status = XML_Parse(_handle, mmm.AddrOfPinnedObject(), length, length == 0);

            if (status == XmlStatus.Error)
                throw new ExpatException(GetLastError());
        }
        finally
        {
            if (mmm.IsAllocated)
                mmm.Free();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Reset()
    {
        ThrowIfDisposed();

        if (XML_ParserReset(_handle, _encoding) == 0)
            throw new ExpatException(GetLastError());

        // since all events are cleared from parser we must rebind them.
        BindParserEvents();
    }

    public void Dispose()
    {
        ThrowIfDisposed();
        _disposed = true;

        if (_encoding != nint.Zero)
        {
            Marshal.FreeHGlobal(_encoding);
            _encoding = nint.Zero;
        }

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

        _content?.Clear();
        _content = null;

        GC.SuppressFinalize(this);
    }

    protected void OnElementStartCallback(nint _, nint namePtr, nint attrListPtr)
    {
        var tagName = Marshal.PtrToStringAnsi(namePtr);

        var temp = new string[XML_GetSpecifiedAttributeCount(_handle)];

        nint ptr;
        int ofs = 0;

        while ((ptr = Marshal.ReadIntPtr(attrListPtr, ofs * nint.Size)) != 0)
        {
            temp[ofs] = Marshal.PtrToStringAnsi(ptr);
            ofs++;
        }

        var attrs = new Dictionary<string, string>();

        for (int i = 0; i < temp.Length; i += 2)
            attrs[temp[i]] = temp[i + 1];

        OnElementStart?.Invoke(this, new()
        {
            TagName = tagName,
            Attributes = attrs,
            Depth = _depth++
        });
    }

    protected void OnElementEndCallback(nint _, nint namePtr)
    {
        var tagName = Marshal.PtrToStringAnsi(namePtr);

        OnElementEnd?.Invoke(this, new()
        {
            TagName = tagName,
            Depth = --_depth
        });

        if (_depth < 0)
            Debug.WriteLine("Warning: Negative parser depth reached?\n" + Environment.StackTrace);
    }

    protected void OnCommentCallback(nint _, nint commentPtr)
    {
        var str = Marshal.PtrToStringAnsi(commentPtr);
        OnComment?.Invoke(this, new() { Value = str });
    }

    protected unsafe void OnCharacterDataCallback(nint _, nint dataPtr, int len)
    {
        var text = Encoding.UTF8.GetString((byte*)dataPtr, len);

        if (_isCdata)
            _content.Append(text);
        else
            OnText?.Invoke(this, new() { Value = text });
    }

    protected void OnProcessingInstructionCallback(nint _, nint targetPtr, nint dataPtr)
    {
        var target = Marshal.PtrToStringAnsi(targetPtr);
        var data = Marshal.PtrToStringAnsi(dataPtr);

        OnProcessingInstruction?.Invoke(this, new()
        {
            Target = target,
            Data = data
        });
    }

    protected void OnCdataStartCallback(nint _)
    {
        _isCdata = true;
        _content.Clear();
    }

    protected void OnCdataEndCallback(nint _)
    {
        var result = _content.ToString();
        OnCdata?.Invoke(this, new() { Value = result });
        _content.Clear();
        _isCdata = false;
    }

    protected void OnPrologCallback(nint _, nint versionPtr, nint encodingPtr, int standalone)
    {
        var version = Marshal.PtrToStringAnsi(versionPtr);
        var encoding = Marshal.PtrToStringAnsi(encodingPtr);

        OnProlog?.Invoke(this, new()
        {
            Version = version,
            Encoding = encoding,
            Standalone = standalone switch
            {
                1 => true,
                0 => false,
                _ => null
            }
        });
    }
}
