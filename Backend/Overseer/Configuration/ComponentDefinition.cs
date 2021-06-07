#nullable disable // JSON + nullable sucks...

namespace Overseer.Configuration
{
    internal class ComponentDefinition
    {
        public string Application { get; set; }
        public bool LaunchOnStart { get; set; } = true;
    }
}