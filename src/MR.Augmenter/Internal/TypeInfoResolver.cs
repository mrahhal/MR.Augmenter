using System;
using System.Reflection;

namespace MR.Augmenter.Internal
{
	public static class TypeInfoResolver
	{
		public static TypeInfoWrapper ResolveTypeInfo(Type type)
		{
			var isArray = false;
			var isWrapper = false;
			var isPrimitive = false;
			Type elementType;

			if (ReflectionHelper.IsPrimitive(type))
			{
				return new TypeInfoWrapper(type, false, false, true);
			}

			if (!(isArray = ReflectionHelper.IsEnumerableOrArrayType(type, out elementType)))
			{
				elementType = type;
			}

			if (elementType != type)
			{
				if (ReflectionHelper.IsPrimitive(elementType))
				{
					isPrimitive = true;
				}
			}

			var ti = elementType.GetTypeInfo();
			if (typeof(AugmenterWrapper).GetTypeInfo().IsAssignableFrom(ti))
			{
				isWrapper = true;
				elementType = ti.GenericTypeArguments[0];
			}

			return new TypeInfoWrapper(elementType, isArray, isWrapper, isPrimitive);
		}
	}
}
