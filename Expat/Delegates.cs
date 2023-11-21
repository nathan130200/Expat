using Expat.EventArgs;
using System.Runtime.InteropServices;

namespace Expat;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void XML_XmlDeclHandler(nint userData, nint versionPtr, nint encodingPtr, int standalone);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void XML_StartElementHandler(nint userData, nint namePtr, nint attrListPtr);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void XML_EndElementHandler(nint userData, nint namePtr);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void XML_CharacterDataHandler(nint userData, nint dataPtr, int len);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void XML_CommentHandler(nint userData, nint dataPtr);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void XML_CdataSectionHandler(nint userData);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void XML_ProcessingInstructionHandler(nint userData, nint dataPtr, nint targetPtr);

public delegate void ParserEventHandler<in TEventArgs>(TEventArgs e)
    where TEventArgs : ParserEventArgs;