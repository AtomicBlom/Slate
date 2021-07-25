using BinaryVibrance.MonoGame.Input.Keyboard;
using Microsoft.Xna.Framework.Input;

namespace BinaryVibrance.MonoGame.Input.Binding.Button
{
	internal abstract class ButtonBindingDefinition : IStartButtonBindingDefinition
	{
		private readonly ButtonBinding _buttonBinding;

		protected ButtonBindingDefinition(ButtonBinding buttonBinding)
		{
			_buttonBinding = buttonBinding;
		}

		public IKeyboardButtonBindingOrNewButtonBindingDefinition KeyBinding(Keys key) => _buttonBinding.KeyBinding(key);
		//public IMouseButtonBindingOrNewButtonBindingDefinition MouseBinding(MouseButton mouseButton) => _buttonBinding.MouseBinding(mouseButton);
	}
}