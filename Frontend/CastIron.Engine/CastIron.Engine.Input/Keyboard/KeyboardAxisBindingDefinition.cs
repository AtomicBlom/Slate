using CastIron.Engine.Input.Binding.Axis;

namespace CastIron.Engine.Input.Keyboard
{
	internal class KeyboardAxisBindingDefinition : AxisBindingDefinition, IKeyboardAxisBindingOrNewAxisBindingDefinition
	{
		private readonly KeyboardAxisBinding _keyboardAxisBinding;
		public KeyboardAxisBindingDefinition(AxisBinding axisBinding, KeyboardAxisBinding keyboardAxisBinding) : base(axisBinding)
		{
			_keyboardAxisBinding = keyboardAxisBinding;
		}
		public void WithAlt()
		{
			_keyboardAxisBinding.RequireAlt = true;
		}
	}
}