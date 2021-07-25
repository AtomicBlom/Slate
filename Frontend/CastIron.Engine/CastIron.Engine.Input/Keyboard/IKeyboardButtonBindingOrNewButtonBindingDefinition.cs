using CastIron.Engine.Input.Binding.Button;

namespace CastIron.Engine.Input.Keyboard
{
	public interface IKeyboardButtonBindingOrNewButtonBindingDefinition : IStartButtonBindingDefinition
	{
		void WithAlt();
	}
}