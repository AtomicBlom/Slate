using System;
using System.Diagnostics;
using CastIron.Engine.Debugging;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CastIron.Engine.Camera
{
    /// <summary>
    /// This is a camera i basically remade to make it work. 
    /// Using quite a bit of stuff from my camera class its nearly the same as mine but its a bit simpler. 
    /// I have bunches of cameras at this point and i need to combine them into a fully hard core non basic camera.
    /// That said simple makes for a better example and a better basis to combine them later.
    /// </summary>
    /// <remarks>
    /// This was sourced from http://community.monogame.net/t/fixed-and-free-3d-camera-code-example/11476
    /// </remarks>
    [PublicAPI]
    public class FreeRoamingCamera : ICameraBehaviour
    {
        private GraphicsDevice? _graphicsDevice;
        
        public float FieldOfViewDegrees { get; set; } = 80f;
        public float NearClipPlane { get; set; }= .05f;
        public float FarClipPlane { get; set; }= 2000f;

        public void SetGraphicsDevice(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            ReCreateWorldAndView();
            ReCreateThePerspectiveProjectionMatrix(FieldOfViewDegrees, NearClipPlane, FarClipPlane);
        }

        /// <summary>
        /// This serves as the cameras up. For fixed cameras this might not change at all ever. For free cameras it changes constantly.
        /// A fixed camera keeps a fixed horizon but can gimble lock under normal rotation when looking straight up or down.
        /// A free camera has no fixed horizon but can't gimble lock under normal rotation as the up changes as the camera moves.
        /// Most hybrid cameras are a blend of the two but all are based on one or both of the above.
        /// </summary>
        private Vector3 _up = Vector3.UnitZ;
        /// <summary>
        /// This serves as the cameras world orientation like almost all 3d game objects they have a world matrix. 
        /// It holds all values that determine orientation and is used to move the camera properly thru the world space.
        /// </summary>
        private Matrix _camerasWorld = Matrix.Identity;

        /// <summary>
        /// Gets or sets the the camera's position in the world.
        /// </summary>
        public Vector3 Position
        {
            set
            {
                _camerasWorld.Translation = value;
                // since we know here that a change has occured to the cameras world orientations we can update the view matrix.
                ReCreateWorldAndView();
            }
            get => _camerasWorld.Translation;
        }
        /// <summary>
        /// Gets or Sets the direction the camera is looking at in the world.
        /// The forward is the same as the look at direction it is a directional vector not a position.
        /// </summary>
        public Vector3 Forward
        {
            set
            {
                _camerasWorld = Matrix.CreateWorld(_camerasWorld.Translation, value, _up);
                // since we know here that a change has occured to the cameras world orientations we can update the view matrix.
                ReCreateWorldAndView();
            }
            get => _camerasWorld.Forward;
        }
        /// <summary>
        /// Get the cameras up vector. You shouldn't need to set the up you shouldn't at all if you are using the free camera type.
        /// </summary>
        public Vector3 Up
        {
            set
            {
                _up = value;
                _camerasWorld = Matrix.CreateWorld(_camerasWorld.Translation, _camerasWorld.Forward, value);
                // since we know here that a change has occured to the cameras world orientations we can update the view matrix.
                ReCreateWorldAndView();
            }
            get => _up;
        }

        /// <summary>
        /// Gets or Sets the direction the camera is looking at in the world as a directional vector.
        /// </summary>
        public Vector3 LookAtDirection
        {
            set
            {
                _camerasWorld = Matrix.CreateWorld(_camerasWorld.Translation, value, _up);
                // since we know here that a change has occured to the cameras world orientations we can update the view matrix.
                ReCreateWorldAndView();
            }
            get => _camerasWorld.Forward;
        }

        /// <summary>
        /// Directly set or get the world matrix this also updates the view matrix
        /// </summary>
        public Matrix World
        {
            get => _camerasWorld;
            set
            {
                _camerasWorld = value;
                View = Matrix.CreateLookAt(_camerasWorld.Translation, _camerasWorld.Forward + _camerasWorld.Translation, _camerasWorld.Up);
            }
        }

        public void NotifyDebugInfo(IDebugInfoSink debugInfoSink)
        {
            World.Decompose(out _, out var rotation, out var translation);
            debugInfoSink.AddDebugInfo(DebugInfoCorner.TopLeft, "Camera")
                .Add("position", $"(x: {translation.X:0.000}, y: {translation.Y:0.000}, z: {translation.Z:0.000})")
                .Add("rotation", $"(x: {rotation.X:0.000}, y: {rotation.Y:0.000}, z: {rotation.Z:0.000})");
        }

        /// <summary>
        /// Gets the view matrix we never really set the view matrix ourselves outside this method just get it.
        /// The view matrix is remade internally when we know the world matrix forward or position has been changed.
        /// </summary>
        public Matrix View { get; private set; } = Matrix.Identity;

        /// <summary>
        /// Gets the projection matrix.
        /// </summary>
        public Matrix Projection { get; private set; } = Matrix.Identity;

        /// <summary>
        /// When the cameras position or orientation changes, we call this to ensure that the cameras world matrix is orthanormal.
        /// We also set the up depending on our choices of is fix or free camera and we then update the view matrix.
        /// </summary>
        private void ReCreateWorldAndView()
        {
            _up = _camerasWorld.Up;
            _camerasWorld = Matrix.CreateWorld(_camerasWorld.Translation, _camerasWorld.Forward, _up);
            View = Matrix.CreateLookAt(_camerasWorld.Translation, _camerasWorld.Forward + _camerasWorld.Translation, _camerasWorld.Up);
        }
        
        /// <summary>
        /// Changes the perspective matrix to a new near far and field of view.
        /// The projection matrix is typically only set up once at the start of the app.
        /// </summary>
        private void ReCreateThePerspectiveProjectionMatrix(float fieldOfViewInDegrees, float nearPlane, float farPlane)
        {
            Debug.Assert(_graphicsDevice != null, "_graphicsDevice != null");

            // create the projection matrix.
            FieldOfViewDegrees = MathHelper.ToRadians(fieldOfViewInDegrees);
            NearClipPlane = nearPlane;
            FarClipPlane = farPlane;
            var aspectRatio = _graphicsDevice.Viewport.Width / (float)_graphicsDevice.Viewport.Height;
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfViewDegrees, aspectRatio, NearClipPlane, FarClipPlane);
        }
        
        public void MoveForwardBackward(float units, GameTime gameTime)
        {
            if (Math.Abs(units) < float.Epsilon) return;
	        Position += _camerasWorld.Forward * units * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public void MoveLeftRight(float units, GameTime gameTime)
        {
	        if (Math.Abs(units) < float.Epsilon) return;
            Position += _camerasWorld.Left * units * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void MoveUpDown(float units, GameTime gameTime)
        {
            if (Math.Abs(units) < float.Epsilon) return;
            Position += _camerasWorld.Up * units * (float) gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void RotateLeftOrRight(float amount, GameTime gameTime)
        {
            var radians = amount * (float)gameTime.ElapsedGameTime.TotalSeconds;
            var matrix = Matrix.CreateFromAxisAngle(_camerasWorld.Up, MathHelper.ToRadians(radians));
            LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
            ReCreateWorldAndView();
        }
        public void RotateUpOrDown(float amount, GameTime gameTime)
        {
            var radians = amount * (float)gameTime.ElapsedGameTime.TotalSeconds;
            var matrix = Matrix.CreateFromAxisAngle(_camerasWorld.Right, MathHelper.ToRadians(radians));
            LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
            ReCreateWorldAndView();
        }

        public void RotateSideToSide(float amount, GameTime gameTime)
        {
	        var radians = amount * (float)gameTime.ElapsedGameTime.TotalSeconds;
	        var pos = _camerasWorld.Translation;
	        _camerasWorld *= Matrix.CreateFromAxisAngle(_camerasWorld.Forward, MathHelper.ToRadians(radians));
	        _camerasWorld.Translation = pos;
	        ReCreateWorldAndView();
        }
    }
}