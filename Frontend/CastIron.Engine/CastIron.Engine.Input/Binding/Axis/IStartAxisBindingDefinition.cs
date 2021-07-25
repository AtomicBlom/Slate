using CastIron.Engine.Input.Keyboard;
using CastIron.Engine.Input.Mouse;
using Microsoft.Xna.Framework.Input;

namespace CastIron.Engine.Input.Binding.Axis
{
	public interface IStartAxisBindingDefinition
	{
		IKeyboardAxisBindingOrNewAxisBindingDefinition KeyBinding(Keys key, float sensitivity);
		IMouseAxisBindingOrNewAxisBindingDefinition MouseBinding(MouseAxis mouseAxis);
	}
}