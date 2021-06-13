#nullable disable // JSON + nullable sucks...
namespace Overseer.Configuration
{
    internal class ComponentSection
    {
        public string ComponentRootPath { get; set; }
        public ComponentDefinition[] Definitions { get; set; }
    }
}