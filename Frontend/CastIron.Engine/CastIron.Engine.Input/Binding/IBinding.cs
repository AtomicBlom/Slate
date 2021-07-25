using Microsoft.Xna.Framework;

namespace CastIron.Engine.Input.Binding
{
	internal interface IBinding
	{
		void UpdateState(GameWindow gameWindow, InputSettings settings);
		
	}
}