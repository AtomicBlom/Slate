using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CastIron.Engine.Input
{
	/// <summary>
	/// https://stackoverflow.com/a/26281533
	/// </summary>
	/// <typeparam name="TEnum"></typeparam>
	internal class FastEnumIntEqualityComparer<TEnum> : IEqualityComparer<TEnum> 
		where TEnum : struct, Enum
	{
		private static class BoxAvoidance
		{
			private static readonly Func<TEnum, int> Wrapper;

			public static int ToInt(TEnum enu)
			{
				return Wrapper(enu);
			}

			static BoxAvoidance()
			{
				var p = Expression.Parameter(typeof(TEnum), null);
				var c = Expression.ConvertChecked(p, typeof(int));

				Wrapper = Expression.Lambda<Func<TEnum, int>>(c, p).Compile();
			}
		}

		public bool Equals(TEnum firstEnum, TEnum secondEnum)
		{
			return BoxAvoidance.ToInt(firstEnum) ==
			       BoxAvoidance.ToInt(secondEnum);
		}

		public int GetHashCode(TEnum firstEnum)
		{
			return BoxAvoidance.ToInt(firstEnum);
		}
	}
}