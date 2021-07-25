using CastIron.Engine.Input.Keyboard;
using Microsoft.Xna.Framework.Input;

namespace CastIron.Engine.Input.Binding.Button
{
	public interface IStartButtonBindingDefinition
	{
		IKeyboardButtonBindingOrNewButtonBindingDefinition KeyBinding(Keys key);
	}
}