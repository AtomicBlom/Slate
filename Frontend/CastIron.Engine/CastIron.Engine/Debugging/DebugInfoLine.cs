using System.Collections;
using System.Collections.Generic;

namespace CastIron.Engine.Debugging
{
	public class DebugInfoLine : IEnumerable<DebugItem>
	{
		private readonly List<DebugItem> _section = new();
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
			if (item is not string text)
			{
                text = item?.ToString() ?? string.Empty;
            }

			_section.Add(new DebugItem(header, text));

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