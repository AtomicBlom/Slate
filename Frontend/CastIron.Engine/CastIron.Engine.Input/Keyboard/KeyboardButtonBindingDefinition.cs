using CastIron.Engine.Input.Binding.Button;

namespace CastIron.Engine.Input.Keyboard
{
	internal class KeyboardButtonBindingDefinition : ButtonBindingDefinition, IKeyboardButtonBindingOrNewButtonBindingDefinition
	{
		private readonly KeyboardButtonBinding _keyboardAxisBinding;
		public KeyboardButtonBindingDefinition(ButtonBinding buttonBinding, KeyboardButtonBinding keyboardAxisBinding) : base(buttonBinding)
		{
			_keyboardAxisBinding = keyboardAxisBinding;
		}
		public void WithAlt()
		{
			_keyboardAxisBinding.RequireAlt = true;
		}
	}
}