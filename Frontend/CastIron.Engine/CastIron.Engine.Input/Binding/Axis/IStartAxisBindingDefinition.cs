using BinaryVibrance.MonoGame.Input.Keyboard;
using BinaryVibrance.MonoGame.Input.Mouse;
using Microsoft.Xna.Framework.Input;

namespace BinaryVibrance.MonoGame.Input.Binding.Axis
{
	public interface IStartAxisBindingDefinition
	{
		IKeyboardAxisBindingOrNewAxisBindingDefinition KeyBinding(Keys key, float sensitivity);
		IMouseAxisBindingOrNewAxisBindingDefinition MouseBinding(MouseAxis mouseAxis);
	}
}