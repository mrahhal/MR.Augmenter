using System;
using System.Collections.Generic;
using System.Reflection;

namespace MR.Augmenter.Internal
{
	internal static class ReflectionHelper
	{
		public static List<Type> IncludeBaseTypes(Type type, bool withSelf = false)
		{
			var list = new List<Type>();

			if (withSelf)
			{
				list.Add(type);
			}

			TypeInfo pivot = type.GetTypeInfo();
			while (true)
			{
				pivot = pivot.BaseType.GetTypeInfo();
				if (pivot.AsType() == typeof(object))
				{
					break;
				}

				list.Add(pivot.AsType());
			}

			list.Reverse();
			return list;
		}

		public static bool IsPrimitive(Type type)
		{
			return
				type.GetTypeInfo().IsPrimitive ||
				type == typeof(string) ||
				type == typeof(DateTime) ||
				type == typeof(DateTimeOffset);
		}
	}
}
