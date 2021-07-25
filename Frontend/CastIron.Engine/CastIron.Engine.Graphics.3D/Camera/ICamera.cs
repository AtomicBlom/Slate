using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace CastIron.Engine.Camera
{
    [PublicAPI]
    public interface ICamera
    {
        Matrix View { get; }
        Matrix Projection { get; }
        Matrix ViewProjection { get; }
        Matrix World { get; }

        ICameraBehaviour? CameraBehaviour { get; set; }
    }
}