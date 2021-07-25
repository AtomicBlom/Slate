using CastIron.Engine.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CastIron.Engine.Graphics.Camera
{
    public interface ICameraBehaviour
    {
        Matrix View { get; }
        Matrix Projection { get; }
        Matrix World { get; }
        void SetGraphicsDevice(GraphicsDevice graphicsDevice);
        void NotifyDebugInfo(IDebugInfoSink debugInfoSink);
    }
}