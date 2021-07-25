using BinaryVibrance.MonoGame.Input.Binding.Button;

namespace BinaryVibrance.MonoGame.Input.Keyboard
{
	public interface IKeyboardButtonBindingOrNewButtonBindingDefinition : IStartButtonBindingDefinition
	{
		void WithAlt();
	}
}