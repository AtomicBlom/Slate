using Microsoft.Xna.Framework;

namespace BinaryVibrance.MonoGame.Input.Binding
{
	internal interface IBinding
	{
		void UpdateState(GameWindow gameWindow, InputSettings settings);
		
	}
}