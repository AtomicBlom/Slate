using BinaryVibrance.MonoGame.Input.Keyboard;
using BinaryVibrance.MonoGame.Input.Mouse;
using Microsoft.Xna.Framework.Input;

namespace BinaryVibrance.MonoGame.Input.Binding.Axis
{
	internal abstract class AxisBindingDefinition : IStartAxisBindingDefinition
	{
		private readonly AxisBinding _axisBinding;

		protected AxisBindingDefinition(AxisBinding axisBinding)
		{
			_axisBinding = axisBinding;
		}

		public IKeyboardAxisBindingOrNewAxisBindingDefinition KeyBinding(Keys key, float sensitivity) => _axisBinding.KeyBinding(key, sensitivity);
		public IMouseAxisBindingOrNewAxisBindingDefinition MouseBinding(MouseAxis mouseAxis) => _axisBinding.MouseBinding(mouseAxis);
	}
}