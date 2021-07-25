namespace CastIron.Engine.Input.Binding.Axis
{
	internal interface IAxisBinding : IBinding
	{ 
		float AxisChangeAmount { get; }
	}
}