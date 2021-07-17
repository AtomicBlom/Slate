using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using MLEM.Ui.Elements;

namespace Slate.Client.UI.MVVM.Binding
{
    public static class ElementExtensions
	{
		public static TElement AddChildren<TElement>(this TElement parent, params Element[] children)
			where TElement : Element
		{
			foreach (var element in children) parent.AddChild(element);

			return parent;
		}
    }
}