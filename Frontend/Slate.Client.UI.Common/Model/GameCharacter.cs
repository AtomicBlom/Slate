using System;

namespace Slate.Client.UI.Common.Model
{
    public record GameCharacter(Guid Id, string Name)
    {
        public string IdAsString => Id.ToString();
    }
}