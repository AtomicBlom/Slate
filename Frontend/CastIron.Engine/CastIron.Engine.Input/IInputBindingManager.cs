using System;
using CastIron.Engine.Input.Binding.Axis;
using CastIron.Engine.Input.Binding.Button;
using JetBrains.Annotations;

namespace CastIron.Engine.Input
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