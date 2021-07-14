using System;

namespace BinaryVibrance.INPCSourceGenerator
{
    [Flags]
    public enum PropertyAccess
    {
        Public = 0,
        GetterPrivate = 1,
        GetterWriteonly = 2,
        SetterPrivate = 4,
        SetterReadonly = 8
    }
}