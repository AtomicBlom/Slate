using System;
using BinaryVibrance.MonoGame.Input.Binding.Axis;
using Microsoft.Xna.Framework;

namespace BinaryVibrance.MonoGame.Input.Mouse
{
    internal class MouseAxisBinding : IMouseAxisBinding, IAxisBinding
    {
        private readonly MouseAxis _mouseAxis;
        private readonly float _sensitivity;
        private float _previousValue;
        private float _axisAmount;

        public MouseAxisBinding(MouseAxis mouseAxis, float sensitivity = 1.0f)
        {
	        _mouseAxis = mouseAxis;
	        _sensitivity = sensitivity;
        }

        public void UpdateState(GameWindow gameWindow, InputSettings settings)
        {
            var state = Microsoft.Xna.Framework.Input.Mouse.GetState();
            var currentValue = _mouseAxis switch
            {
                MouseAxis.Horizontal => state.X,
                MouseAxis.Vertical => state.Y,
                MouseAxis.VerticalScrollWheel => state.ScrollWheelValue,
                MouseAxis.HorizontalScrollWheel => state.HorizontalScrollWheelValue,
                _ => throw new ArgumentOutOfRangeException()
            };

            var previousValue = _mouseAxis switch
            {
                MouseAxis.Horizontal when settings.LockMouseToScreenCenter => gameWindow.ClientBounds.Width / 2.0f,
                MouseAxis.Vertical when settings.LockMouseToScreenCenter => gameWindow.ClientBounds.Height / 2.0f,
                _ => _previousValue
            };

            var amount = previousValue - currentValue;

            if (Inverted) amount *= -1;
            _axisAmount = amount;
            _previousValue = currentValue;
        }

        public float AxisChangeAmount => _axisAmount * _sensitivity;
        public bool Inverted { get; set; }
    }
}