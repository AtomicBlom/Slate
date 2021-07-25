namespace BinaryVibrance.MonoGame.Input.Binding.Axis
{
	internal interface IAxisBinding : IBinding
	{ 
		float AxisChangeAmount { get; }
	}
}