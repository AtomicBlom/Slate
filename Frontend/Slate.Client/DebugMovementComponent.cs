using CastIron.Engine.Graphics.Camera;
using CastIron.Engine.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Slate.Client
{
    internal class DebugMovementComponent : SimpleGameComponent
    {
        private readonly IInputBindingManager<GameInputState> _inputManager;
        private readonly ICamera _camera;
        private readonly FreeRoamingCamera _debugCameraBehaviour;

        public DebugMovementComponent(IInputBindingManager<GameInputState> inputManager, ICamera camera)
        {
            _inputManager = inputManager;
            _camera = camera;
            _debugCameraBehaviour = new FreeRoamingCamera();
        }

        public override void Update(GameTime gameTime)
        {
            if (_inputManager.Button(MainMenuAction.EnterDebugMode).Released)
            {
                _inputManager.CurrentState = GameInputState.Debug;
                _camera.CameraBehaviour = _debugCameraBehaviour;
            }

            if (_inputManager.Button(DebugControls.ExitDebugMode).Released)
            {
                _inputManager.CurrentState = GameInputState.MainMenu;
                _camera.CameraBehaviour = null;
            }

            if (_inputManager.CurrentState == GameInputState.Debug)
            {
                _debugCameraBehaviour.MoveLeftRight(_inputManager.GetAxis(DebugControls.MoveLeftRightAxis).Value, gameTime);
                _debugCameraBehaviour.MoveForwardBackward(_inputManager.GetAxis(DebugControls.MoveForwardBackAxis).Value, gameTime);
                _debugCameraBehaviour.MoveUpDown(_inputManager.GetAxis(DebugControls.MoveUpDownAxis).Value, gameTime);

                _debugCameraBehaviour.RotateUpOrDown(_inputManager.GetAxis(DebugControls.PitchAxis).Value, gameTime);
                _debugCameraBehaviour.RotateLeftOrRight(_inputManager.GetAxis(DebugControls.YawAxis).Value, gameTime);
                _debugCameraBehaviour.RotateSideToSide(_inputManager.GetAxis(DebugControls.RollAxis).Value, gameTime);
            }
        }
    }
}