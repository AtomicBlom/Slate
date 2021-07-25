using System;
using System.Collections;
using System.Collections.Generic;

namespace CastIron.Engine.Debugging
{
	public class DebugInfoSection : IEnumerable<DebugInfoLine>
	{
		public DebugInfoSection(bool reversed)
		{
			if (reversed)
			{
				var collection = new Stack<DebugInfoLine>();
				Collection = collection;
				Add = item => collection.Push(item);
				Clear = () => collection.Clear();
			}
			else
			{
				var collection = new Queue<DebugInfoLine>();
				Collection = collection;
				Add = item => collection.Enqueue(item);
				Clear = () => collection.Clear();
			}
		}

		private IEnumerable<DebugInfoLine> Collection { get; }

		public Action<DebugInfoLine> Add { get; }
		public Action Clear { get; }
		public IEnumerator<DebugInfoLine> GetEnumerator()
		{
			return Collection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}