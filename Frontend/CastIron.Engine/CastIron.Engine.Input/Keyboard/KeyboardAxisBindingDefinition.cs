using BinaryVibrance.MonoGame.Input.Binding.Axis;

namespace BinaryVibrance.MonoGame.Input.Keyboard
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