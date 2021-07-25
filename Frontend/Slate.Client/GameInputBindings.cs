using BinaryVibrance.MonoGame.Input;
using BinaryVibrance.MonoGame.Input.Mouse;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Slate.Client
{
    public enum GlobalControls
    {
        SwitchFullscreen,
        ForceExit
    }

    public enum InGameAction
    {
        MoveForwardBackAxis,
        MoveLeftRightAxis,
        EnterDebugMode
    }

    internal enum GameInputState
    {
        Global,
        MainMenu,
        InGame,
        Debug
    }

    public enum DebugControls
    {
        ExitDebugMode,
        MoveForwardBackAxis,
        MoveLeftRightAxis,
        MoveUpDownAxis,
        PitchAxis,
        YawAxis,
        NewLevel,
        FocusRoom,
        UnFocusRoom,
        RollAxis
    }

    internal static class GameInputBindings
    {
        internal static InputBindingManager<GameInputState> CreateInputBindings(Game game)
        {
            var moveSpeed = 30.0f;
            var lookSpeed = 30.0f;

            var inputManager = new InputBindingManager<GameInputState>(game, GameInputState.Global);

            var inGameBindings = inputManager.DefineGameState<InGameAction>(GameInputState.InGame)
                .LockMouseToScreenCenter();
            inGameBindings.DefineAxisBinding(InGameAction.MoveForwardBackAxis)
                .KeyBinding(Keys.W, 1.0f).KeyBinding(Keys.S, -1.0f)
                .KeyBinding(Keys.Up, 1.0f).KeyBinding(Keys.Down, -1.0f);
            inGameBindings.DefineAxisBinding(InGameAction.MoveLeftRightAxis)
                .KeyBinding(Keys.A, 1.0f).KeyBinding(Keys.D, -1.0f)
                .KeyBinding(Keys.Left, 1.0f).KeyBinding(Keys.Right, -1.0f);
            inGameBindings.DefineButtonBinding(InGameAction.EnterDebugMode)
                .KeyBinding(Keys.OemTilde).WithAlt();


            var globalBindings = inputManager.DefineGameState<GlobalControls>(GameInputState.Global);
            globalBindings.DefineButtonBinding(GlobalControls.SwitchFullscreen)
                .KeyBinding(Keys.Enter).WithAlt();
            globalBindings.DefineButtonBinding(GlobalControls.ForceExit)
                .KeyBinding(Keys.Escape);


            var debugBindings = inputManager.DefineGameState<DebugControls>(GameInputState.Debug)
                .LockMouseToScreenCenter();
            debugBindings.DefineButtonBinding(DebugControls.ExitDebugMode)
                .KeyBinding(Keys.OemTilde).WithAlt();
            debugBindings.DefineButtonBinding(DebugControls.NewLevel)
                .KeyBinding(Keys.P);
            debugBindings.DefineButtonBinding(DebugControls.FocusRoom).KeyBinding(Keys.F);
            debugBindings.DefineButtonBinding(DebugControls.UnFocusRoom).KeyBinding(Keys.G);
            debugBindings.DefineAxisBinding(DebugControls.MoveForwardBackAxis)
                .ScaledTo(moveSpeed)
                .KeyBinding(Keys.W, 1.0f).KeyBinding(Keys.S, -1.0f)
                .KeyBinding(Keys.Up, 1.0f).KeyBinding(Keys.Down, -1.0f);
            debugBindings.DefineAxisBinding(DebugControls.MoveLeftRightAxis)
                .ScaledTo(moveSpeed)
                .KeyBinding(Keys.A, 1.0f).KeyBinding(Keys.D, -1.0f)
                .KeyBinding(Keys.Left, 1.0f).KeyBinding(Keys.Right, -1.0f);
            debugBindings.DefineAxisBinding(DebugControls.MoveUpDownAxis)
                .ScaledTo(moveSpeed)
                .KeyBinding(Keys.LeftControl, -1.0f).KeyBinding(Keys.Space, 1.0f);
            debugBindings.DefineAxisBinding(DebugControls.RollAxis)
                .ScaledTo(lookSpeed)
                .KeyBinding(Keys.Q, -1.0f).KeyBinding(Keys.E, 1.0f);
            debugBindings.DefineAxisBinding(DebugControls.PitchAxis)
                .ScaledTo(lookSpeed)
                .MouseBinding(MouseAxis.Vertical).Inverted();
            debugBindings.DefineAxisBinding(DebugControls.YawAxis)
                .ScaledTo(lookSpeed)
                .MouseBinding(MouseAxis.Horizontal);


            inputManager.CurrentState = GameInputState.InGame;
            return inputManager;
        }
    }
}
