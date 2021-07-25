using CastIron.Engine.Input.Binding.Axis;
using JetBrains.Annotations;

namespace CastIron.Engine.Input.Keyboard
{
    [PublicAPI]
	public interface IKeyboardAxisBindingOrNewAxisBindingDefinition : IStartAxisBindingDefinition
	{
		void WithAlt();
	}
}