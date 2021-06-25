using System;

#nullable disable // JSON + nullable sucks...

namespace Slate.Overseer.Configuration
{
    internal class ComponentDefinition
    {
        public string Name { get; set; }
        public string Application { get; set; }
        public bool LaunchOnStart { get; set; } = true;
        public string[] AdditionalArguments { get; set; } = Array.Empty<string>();
    }
}