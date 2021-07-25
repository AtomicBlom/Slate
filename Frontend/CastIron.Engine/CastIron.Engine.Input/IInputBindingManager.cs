using System;
using BinaryVibrance.MonoGame.Input.Binding.Axis;
using BinaryVibrance.MonoGame.Input.Binding.Button;
using JetBrains.Annotations;

namespace BinaryVibrance.MonoGame.Input
{
	[PublicAPI]
	public interface IInputBindingManager<TState> : IInputBindingManager where TState : struct, Enum
	{
		TState CurrentState { get; set; }
	}

    [PublicAPI]
	public interface IInputBindingManager
	{
		ButtonResult Button<TAction>(TAction buttonControl) where TAction : struct, Enum;
		AxisResult GetAxis<TAction>(TAction axisControl) where TAction : struct, Enum;
	}
}