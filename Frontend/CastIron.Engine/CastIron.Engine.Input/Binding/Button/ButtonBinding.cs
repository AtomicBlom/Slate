using System.Collections.Generic;
using BinaryVibrance.MonoGame.Input.Keyboard;
using Microsoft.Xna.Framework.Input;

namespace BinaryVibrance.MonoGame.Input.Binding.Button
{
	internal class ButtonBinding : IStartButtonBindingDefinition
	{
		private readonly List<IBinding> _bindings;

		public ButtonBinding(List<IBinding> bindingList)
		{
			_bindings = bindingList;
		}

        public IKeyboardButtonBindingOrNewButtonBindingDefinition KeyBinding(Keys key)
		{
			var keyboardButtonBinding = new KeyboardButtonBinding(key);
			_bindings.Add(keyboardButtonBinding);
			return new KeyboardButtonBindingDefinition(this, keyboardButtonBinding);
		}
	}
}