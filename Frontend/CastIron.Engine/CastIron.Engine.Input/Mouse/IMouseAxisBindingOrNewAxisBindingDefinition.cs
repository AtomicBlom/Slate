using CastIron.Engine.Input.Binding.Axis;

namespace CastIron.Engine.Input.Mouse
{
	public interface IMouseAxisBindingOrNewAxisBindingDefinition : IStartAxisBindingDefinition
	{
        void Inverted();
    }
}