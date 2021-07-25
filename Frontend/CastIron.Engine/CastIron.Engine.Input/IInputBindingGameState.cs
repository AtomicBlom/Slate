using System;
using BinaryVibrance.MonoGame.Input.Binding.Axis;
using BinaryVibrance.MonoGame.Input.Binding.Button;

namespace BinaryVibrance.MonoGame.Input
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