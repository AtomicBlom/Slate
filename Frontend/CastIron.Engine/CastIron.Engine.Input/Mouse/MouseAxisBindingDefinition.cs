using CastIron.Engine.Input.Binding.Axis;

namespace CastIron.Engine.Input.Mouse
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