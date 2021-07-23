using System;

namespace Slate.Client.UI.Framework
{
    public record UIPropertyKey(Type FieldType, string Name, Type Owner);
}