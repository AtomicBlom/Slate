using System;

namespace Client.UI.Common.Model
{
    public record GameCharacter(Guid Id, string Name)
    {
        public string IdAsString => Id.ToString();
    }
}