using BinaryVibrance.MonoGame.Input.Keyboard;
using Microsoft.Xna.Framework.Input;

namespace BinaryVibrance.MonoGame.Input.Binding.Button
{
	public interface IStartButtonBindingDefinition
	{
		IKeyboardButtonBindingOrNewButtonBindingDefinition KeyBinding(Keys key);
	}
}