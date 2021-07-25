using BinaryVibrance.MonoGame.Input.Binding.Axis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BinaryVibrance.MonoGame.Input.Keyboard
{
	internal class KeyboardAxisBinding : IAxisBinding
	{
		private readonly Keys _key;
		private readonly float _sensitivity;
		private bool _isAltPressed;
		private KeyState _keyState;
		private float _changeAmount;

		public KeyboardAxisBinding(Keys key, float sensitivity)
		{
			_key = key;
			_sensitivity = sensitivity;
		}

		public void UpdateState(GameWindow gameWindow, InputSettings settings)
		{
			var state = Microsoft.Xna.Framework.Input.Keyboard.GetState();
			_isAltPressed = state[Keys.LeftAlt] == KeyState.Down || state[Keys.RightAlt] == KeyState.Down;
			_keyState = state[_key];

			var isPressed = _keyState == KeyState.Down && (!RequireAlt || _isAltPressed);

			_changeAmount = isPressed ? 1 : 0;
		}
		
		public bool RequireAlt { get; set; }
		public float AxisChangeAmount => _changeAmount * _sensitivity;
	}
}