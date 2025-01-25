using System.Runtime.InteropServices;

namespace Expat;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void StartElementHandler(nint userData, nint tagName_, nint attrList_);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void EndElementHandler(nint userData, nint tagName_);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void CharacterDataHandler(nint userData, nint buf, int len);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void CommentHandler(nint userData, nint data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void CdataSectionHandler(nint userData);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void EntityDeclHandler(nint userData,
    nint entityName_,
    int isParameterEntity,
    nint value_,
    int valueLength,
    nint base_,
    nint systemId_,
    nint publicId_,
    nint notationName_
);