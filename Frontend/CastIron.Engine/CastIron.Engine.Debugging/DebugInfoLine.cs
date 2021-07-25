using System.Collections;
using System.Collections.Generic;

namespace CastIron.Engine.Debugging
{
	public class DebugInfoLine : IEnumerable<DebugItem>
	{
		private readonly List<DebugItem> _section = new List<DebugItem>();
		public string? SectionHeader { get; }

		public DebugInfoLine(string sectionHeader)
		{
			SectionHeader = sectionHeader;
		}

		public DebugInfoLine()
		{
			SectionHeader = null;
		}

		public DebugInfoLine Add<T>(string header, T item)
		{
			if (!(item is string text))
			{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                text = item?.ToString();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }

			_section.Add(new DebugItem(header, text ?? string.Empty));

			return this;
		}

		public IEnumerator<DebugItem> GetEnumerator()
		{
			return _section.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}