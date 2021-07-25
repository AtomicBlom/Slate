using BinaryVibrance.MonoGame.Input.Binding.Button;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BinaryVibrance.MonoGame.Input.Keyboard
{
	internal class KeyboardButtonBinding : IButtonBinding
	{
		private readonly Keys _key;

		private KeyState _keyState;
		private bool _isAltPressed;
		private bool _isKeyDownEvent;
		private bool _isKeyUpEvent;
		
		public KeyboardButtonBinding(Keys key)
		{
			_key = key;
		}

		public void UpdateState(GameWindow gameWindow, InputSettings settings)
		{
			var state = Microsoft.Xna.Framework.Input.Keyboard.GetState();
			_isAltPressed = state[Keys.LeftAlt] == KeyState.Down || state[Keys.RightAlt] == KeyState.Down;
			var previousKeyState = _keyState;
			_keyState = state[_key];
			_isKeyDownEvent = previousKeyState == KeyState.Up && _keyState == KeyState.Down;
			_isKeyUpEvent = previousKeyState == KeyState.Down && _keyState == KeyState.Up;
		}

		public bool Held => _keyState == KeyState.Down && (!RequireAlt || _isAltPressed);
		public bool Pressed => _isKeyDownEvent && (!RequireAlt || _isAltPressed);

		public bool Released => _isKeyUpEvent && (!RequireAlt || _isAltPressed);

		public bool RequireAlt { get; set; }
	}
}