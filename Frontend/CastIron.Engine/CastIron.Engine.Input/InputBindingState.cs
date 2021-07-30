using System;
using System.Collections.Generic;
using System.Linq;
using CastIron.Engine.Input.Binding;
using CastIron.Engine.Input.Binding.Axis;
using CastIron.Engine.Input.Binding.Button;
using Microsoft.Xna.Framework;

namespace CastIron.Engine.Input
{
    internal class InputBindingState<TAction> : IInputBindingGameState<TAction> where TAction: struct, Enum
	{
		private readonly Game _game;

		private readonly Dictionary<TAction, List<IBinding>> _bindings = new(
			Enumerable.Empty<KeyValuePair<TAction, List<IBinding>>>(), 
			new FastEnumIntEqualityComparer<TAction>()
			);
		
		//We can improve on performance and allocations by just iterating over a single list.
		private readonly List<IBinding> _flatBindings = new();
		private InputSettings Settings { get; } = new();

		public InputBindingState(Game game)
		{
			_game = game;
		}

		public IInputBindingGameState<TAction> LockMouseToScreenCenter()
		{
			Settings.LockMouseToScreenCenter = true;
			return this;
		}
		
		public IStartAxisBindingDefinitionOrSensitivity DefineAxisBinding(TAction axisControl)
		{
			if (!_bindings.TryGetValue(axisControl, out var bindingList))
			{
				bindingList = new List<IBinding>();
				_bindings.Add(axisControl, bindingList);
			}

			var axisBinding = new AxisBinding(bindingList);
			return axisBinding;
		}

		public IStartButtonBindingDefinition DefineButtonBinding(TAction buttonControl)
		{
			_flatBindings.Clear();
			if (!_bindings.TryGetValue(buttonControl, out var bindingList))
			{
				bindingList = new List<IBinding>();
				_bindings.Add(buttonControl, bindingList);
			}

			var axisBinding = new ButtonBinding(bindingList);
			return axisBinding;
		}

		public void AggregateState(TAction buttonControl, ref ButtonResult result)
		{
			_flatBindings.Clear();
			if (!_bindings.TryGetValue(buttonControl, out var bindings))
			{
				throw new InputBindingManagerActionMissingException<TAction>(buttonControl);
			}

			for (var i = 0; i < bindings.Count; i++)
			{
				var binding = bindings[i];
				if (binding is IButtonBinding buttonBinding)
				{
					result.Pressed |= buttonBinding.Pressed;
					result.Held |= buttonBinding.Held;
					result.Released |= buttonBinding.Released;
				}
				else
				{
					throw new Exception("Attempted to get the axis of a binding of type: " + binding.GetType());
				}
			}
		}

		public void AggregateState(TAction axisControl, ref AxisResult result)
		{
			if (!_bindings.TryGetValue(axisControl, out var bindings))
			{
				throw new InputBindingManagerActionMissingException<TAction>(axisControl);
			}

			for (var i = 0; i < bindings.Count; i++)
			{
				var binding = bindings[i];
				if (binding is IAxisBinding axisBinding)
				{
					result.Value += axisBinding.AxisChangeAmount;
				}
				else
				{
					throw new Exception("Attempted to get the axis of a binding of type: " + binding.GetType());
				}
			}
		}

		public void UpdateStates()
		{
			if (!_game.IsActive) return;
			
			if (!_flatBindings.Any())
			{
				_flatBindings.AddRange(_bindings.SelectMany(b => b.Value));
			}

			for (var i = 0; i < _flatBindings.Count; i++)
			{
				_flatBindings[i].UpdateState(_game.Window, Settings);
			}

			if (Settings.LockMouseToScreenCenter)
			{
				Microsoft.Xna.Framework.Input.Mouse.SetPosition(_game.Window.ClientBounds.Width / 2, _game.Window.ClientBounds.Height / 2);
			}
		}
	}

    internal class InputSettings
    {
	    public bool LockMouseToScreenCenter { get; set; }
    }
}