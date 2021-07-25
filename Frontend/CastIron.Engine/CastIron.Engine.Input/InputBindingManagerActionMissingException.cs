using System;
using JetBrains.Annotations;

namespace CastIron.Engine.Input
{
	[PublicAPI]
	internal class InputBindingManagerActionMissingException<TAction> : Exception where TAction : struct, Enum
	{
		public TAction Action { get; }

		public InputBindingManagerActionMissingException(TAction action) : base("The requested action is missing from the state")
		{
			Action = action;
		}
	}
}