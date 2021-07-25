using System;
using CastIron.Engine.Input.Binding.Axis;
using CastIron.Engine.Input.Binding.Button;

namespace CastIron.Engine.Input
{
	public interface IInputBindingGameState<in TAction>: IInputBindingGameState where TAction : struct, Enum
	{
		IStartAxisBindingDefinitionOrSensitivity DefineAxisBinding(TAction axisControl);
		IStartButtonBindingDefinition DefineButtonBinding(TAction exitDebugMode);
		void AggregateState(TAction buttonControl, ref ButtonResult result);
		void AggregateState(TAction axisControl, ref AxisResult result);
		IInputBindingGameState<TAction> LockMouseToScreenCenter();
	}

	public interface IInputBindingGameState
	{
		void UpdateStates();
	}
}