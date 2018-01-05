using System;
using System.Collections;
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

			var pivot = type.GetTypeInfo();
			while (true)
			{
				if (pivot.BaseType == null)
				{
					break;
				}

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

		public static List<Type> IncludeBaseTypesAndImplementedInterface(Type type, bool withSelf = false)
		{
			var allTypes = IncludeBaseTypes(type, withSelf);
			allTypes.AddRange(type.GetInterfaces());
			return allTypes;
		}

		public static bool IsPrimitive(Type type)
		{
			var typeInfo = type.GetTypeInfo();
			return
				typeInfo.IsPrimitive ||
				typeInfo.IsValueType ||
				type == typeof(string);
		}

		public static bool IsEnumerableOrArrayType(Type type, out Type elementType)
		{
			var ti = type.GetTypeInfo();

			elementType = ti.GetElementType();
			if (elementType != null)
			{
				return true;
			}

			if (!ti.IsGenericType)
			{
				return false;
			}

			if (!typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(ti))
			{
				return false;
			}

			elementType = ti.GenericTypeArguments[0];
			return true;
		}
	}
}
