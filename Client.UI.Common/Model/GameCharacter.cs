using System;

namespace Networking
{
    public record GameCharacter(Guid Id, string Name)
    {
        public string IdAsString => Id.ToString();
    }
}