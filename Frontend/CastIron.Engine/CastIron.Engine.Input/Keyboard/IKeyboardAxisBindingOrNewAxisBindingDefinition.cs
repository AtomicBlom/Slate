using BinaryVibrance.MonoGame.Input.Binding.Axis;
using JetBrains.Annotations;

namespace BinaryVibrance.MonoGame.Input.Keyboard
{
    [PublicAPI]
	public interface IKeyboardAxisBindingOrNewAxisBindingDefinition : IStartAxisBindingDefinition
	{
		void WithAlt();
	}
}