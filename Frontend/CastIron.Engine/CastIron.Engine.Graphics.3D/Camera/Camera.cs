using CastIron.Engine.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CastIron.Engine.Camera
{
    public class Camera : UpdateableGameComponent, ICamera
    {
        // We need this to calculate the aspectRatio
        // in the ProjectionMatrix property.
        private readonly GraphicsDevice _graphicsDevice;
        private readonly IDebugInfoSink _debugInfoSink;
        private ICameraBehaviour? _cameraBehaviour;


        //public Vector3 LookAtTarget { get; set; } = new Vector3();
        //private Vector3 Rotation { get; set; } = new Vector3();

        public Matrix View => CameraBehaviour?.View ?? Matrix.Identity;

        public Matrix Projection => CameraBehaviour?.Projection ?? Matrix.Identity;

        public Matrix ViewProjection => View * Projection;

        public Matrix World => CameraBehaviour?.World ?? Matrix.Identity;

        public ICameraBehaviour? CameraBehaviour
        {
            get => _cameraBehaviour;
            set
            {
                _cameraBehaviour = value;
                _cameraBehaviour.SetGraphicsDevice(_graphicsDevice);
            }
        }

        public Camera(GraphicsDevice graphicsDevice, IDebugInfoSink debugInfoSink)
        {
            _graphicsDevice = graphicsDevice;
            _debugInfoSink = debugInfoSink;
            //CameraBehaviour = new FreeRoamingCamera(graphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
            if (!_debugInfoSink.Enabled) return;
            CameraBehaviour?.NotifyDebugInfo(_debugInfoSink);
        }
    }
}