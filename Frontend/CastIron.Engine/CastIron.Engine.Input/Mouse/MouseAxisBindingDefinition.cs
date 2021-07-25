using BinaryVibrance.MonoGame.Input.Binding.Axis;

namespace BinaryVibrance.MonoGame.Input.Mouse
{
	internal class MouseAxisBindingDefinition : AxisBindingDefinition, IMouseAxisBindingOrNewAxisBindingDefinition
	{
        private readonly MouseAxisBinding _mouseAxisBinding;
        public MouseAxisBindingDefinition(AxisBinding axisBinding, MouseAxisBinding mouseAxisBinding) : base(axisBinding)
        {
            _mouseAxisBinding = mouseAxisBinding;
        }
        public void Inverted()
        {
            _mouseAxisBinding.Inverted = true;
        }
    }
}