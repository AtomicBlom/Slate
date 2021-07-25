using Microsoft.Xna.Framework;

namespace CastIron.Engine.Debugging
{
    public interface IDebugInfoSink
    {
        DebugInfoLine AddDebugInfo<T>(DebugInfoCorner corner, string header, T text);
        DebugInfoLine AddDebugInfo(DebugInfoCorner corner, string header, Vector3 vector);
        DebugInfoLine AddDebugInfo(DebugInfoCorner corner, string header);
        bool Enabled { get; set; }
    }
}