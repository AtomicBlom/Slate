using BinaryVibrance.MonoGame.Input.Binding.Axis;

namespace BinaryVibrance.MonoGame.Input.Mouse
{
	public interface IMouseAxisBindingOrNewAxisBindingDefinition : IStartAxisBindingDefinition
	{
        void Inverted();
    }
}