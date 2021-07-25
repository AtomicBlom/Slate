using System;
using System.Collections.Generic;
using System.Linq;
using BinaryVibrance.MonoGame.Input.Binding.Axis;
using BinaryVibrance.MonoGame.Input.Binding.Button;
using CastIron.Engine;
using Microsoft.Xna.Framework;

namespace BinaryVibrance.MonoGame.Input
{
	public class InputBindingManager<TState> : UpdateableGameComponent, IInputBindingManager<TState> where TState: struct, Enum
    {
	    private readonly Game _game;
	    private static readonly FastEnumIntEqualityComparer<TState> FastEnumIntEqualityComparer = new FastEnumIntEqualityComparer<TState>();

		private TState GlobalState { get; }
        public TState CurrentState { get; set; }
        
        private readonly Dictionary<TState, IInputBindingGameState> _gameStateBindings = new Dictionary<TState, IInputBindingGameState>(
	        Enumerable.Empty<KeyValuePair<TState, IInputBindingGameState>>(), 
	        FastEnumIntEqualityComparer
	        );
        
        public InputBindingManager(Game game, TState globalState)
	    {
		    _game = game;
		    GlobalState = globalState;
		    CurrentState = GlobalState;
	    }
        
        public override void Update(GameTime gameTime)
        {
	        _gameStateBindings[GlobalState].UpdateStates();

			if (!FastEnumIntEqualityComparer.Equals(CurrentState, GlobalState))
	        {
		        _gameStateBindings[CurrentState].UpdateStates();
	        }
        }

        public IInputBindingGameState<TAction> DefineGameState<TAction>(TState state) where TAction: struct, Enum
        {
	        var keyBindingState = new InputBindingState<TAction>(_game);
	        _gameStateBindings[state] = keyBindingState;
	        return keyBindingState;
        }

        public ButtonResult Button<TControl>(TControl buttonControl) where TControl : struct, Enum
        {
	        var result = new ButtonResult();

			if (_gameStateBindings[CurrentState] is IInputBindingGameState<TControl> currentState)
			{
				currentState.AggregateState(buttonControl, ref result);
			}

			var isGlobalState = FastEnumIntEqualityComparer.Equals(CurrentState, GlobalState);
			if (!isGlobalState && _gameStateBindings[GlobalState] is IInputBindingGameState<TControl> globalState)
			{
				globalState.AggregateState(buttonControl, ref result);
			}

			return result;
		}

        public AxisResult GetAxis<TControl>(TControl axisControl) where TControl : struct, Enum
        {
			var result = new AxisResult();

			if (_gameStateBindings[CurrentState] is IInputBindingGameState<TControl> currentState)
			{
				currentState.AggregateState(axisControl, ref result);
			}

			var isGlobalState = FastEnumIntEqualityComparer.Equals(CurrentState, GlobalState);
			if (!isGlobalState && _gameStateBindings[GlobalState] is IInputBindingGameState<TControl> globalState)
			{
				globalState.AggregateState(axisControl, ref result);
			}

			return result;
		}
    }
}
