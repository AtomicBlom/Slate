using System;
using System.Diagnostics;
using CastIron.Engine.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CastIron.Engine.Graphics.Camera
{
    public class ChaseCamera : ICameraBehaviour
    {
        private GraphicsDevice? _graphicsDevice;
        private float _fieldOfViewDegrees = 45f;
        
        
        private float _nearClipPlane = .05f;
        private float _farClipPlane = 2000f;
        
        private bool _perspectiveDirty;
        private bool _worldDirty;
        private bool _viewDirty;

        private Vector3 _targetLocation;
        private float _followDistance = 30.0f;
        private float _angle = (MathF.PI * 2f) / 30.0f;
        private float _verticalOffset = 30.0f;

        private Matrix _world;
        private Matrix _view;
        private Matrix _projection;

        public Matrix View
        {
            get
            {
                if (!_viewDirty) return _view;
                _viewDirty = false;

                var cameraPosition = _targetLocation + new Vector3(
                    MathF.Cos(_angle) * _followDistance,
                    _verticalOffset,
                    MathF.Sin(_angle) * _followDistance
                    );

                _view = Matrix.CreateLookAt(cameraPosition, _targetLocation, Vector3.Up);

                return _view;
            }
        }

        public Matrix Projection
        {
            get
            {
                if (!_perspectiveDirty) return _projection;
                Debug.Assert(_graphicsDevice != null, "_graphicsDevice != null");
                _perspectiveDirty = false;

                // create the projection matrix.
                var fieldOfView = MathHelper.ToRadians(_fieldOfViewDegrees);
                var aspectRatio = _graphicsDevice.Viewport.Width / (float)_graphicsDevice.Viewport.Height;
                _projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, _nearClipPlane, _farClipPlane);
                
                return _projection;
            }
        }

        public Matrix World
        {
            get
            {
                if (!_worldDirty) return _world;
                _worldDirty = false;
                _world = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
                return _world;
            }
        }
        
        public Vector3 TargetLocation
        {
            get => _targetLocation;
            set => SetWorldViewProperty(ref _targetLocation, value);
        }

        public float FollowDistance
        {
            get => _followDistance;
            set => SetWorldViewProperty(ref _followDistance, value);
        }

        public float VerticalOffset
        {
            get => _verticalOffset;
            set => SetWorldViewProperty(ref _verticalOffset, value);
        }

        public float Angle
        {
            get => _angle;
            set => SetWorldViewProperty(ref _angle, value);
        }

        public float FieldOfViewDegrees
        {
            get => _fieldOfViewDegrees;
            set => SetPerspectiveProperty(ref _fieldOfViewDegrees, value);
        }

        public float NearClipPlane
        {
            get => _nearClipPlane;
            set => SetPerspectiveProperty(ref _nearClipPlane, value);
        }

        public float FarClipPlane
        {
            get => _farClipPlane;
            set => SetPerspectiveProperty(ref _farClipPlane, value);
        }

        public void SetGraphicsDevice(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _perspectiveDirty = true;
            _worldDirty = true;
            _viewDirty = true;
        }

        public void NotifyDebugInfo(IDebugInfoSink debugInfoSink)
        {
            
        }

        private void SetWorldViewProperty<T>(ref T field, T value)
        {
            field = value;
            _worldDirty = true;
            _viewDirty = true;
        }

        private void SetPerspectiveProperty<T>(ref T field, T value)
        {
            field = value;
            _perspectiveDirty = true;
        }
    }
}