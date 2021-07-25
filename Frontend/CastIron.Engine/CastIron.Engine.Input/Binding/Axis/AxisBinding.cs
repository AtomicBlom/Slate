using System.Collections.Generic;
using BinaryVibrance.MonoGame.Input.Keyboard;
using BinaryVibrance.MonoGame.Input.Mouse;
using Microsoft.Xna.Framework.Input;

namespace BinaryVibrance.MonoGame.Input.Binding.Axis
{
	internal class AxisBinding : IStartAxisBindingDefinitionOrSensitivity
	{
		private readonly List<IBinding> _bindings;
        private float _axisScaling = 1.0f;

        public AxisBinding(List<IBinding> bindingList)
		{
			_bindings = bindingList;
		}

		public IKeyboardAxisBindingOrNewAxisBindingDefinition KeyBinding(Keys key, float sensitivity)
		{
			var keyboardAxisBinding = new KeyboardAxisBinding(key, sensitivity * _axisScaling);
			_bindings.Add(keyboardAxisBinding);
			return new KeyboardAxisBindingDefinition(this, keyboardAxisBinding);
		}

		public IMouseAxisBindingOrNewAxisBindingDefinition MouseBinding(MouseAxis mouseAxis)
		{
			var mouseAxisBinding = new MouseAxisBinding(mouseAxis, _axisScaling);
			_bindings.Add(mouseAxisBinding);
			return new MouseAxisBindingDefinition(this, mouseAxisBinding);
		}

        public IStartAxisBindingDefinition ScaledTo(float scale)
        {
			_axisScaling = scale;
            return this;
        }
    }

    public interface IStartAxisBindingDefinitionOrSensitivity : IStartAxisBindingDefinition
    {
        IStartAxisBindingDefinition ScaledTo(float scale);
    }
}