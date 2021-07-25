namespace CastIron.Engine.Debugging
{
	public struct DebugItem
	{
		public readonly string Header;
		public readonly string Text;

		public DebugItem(string header, string text)
		{
			Header = header;
			Text = text;
		}
	}
}