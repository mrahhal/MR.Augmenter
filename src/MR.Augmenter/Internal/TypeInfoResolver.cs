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
			Type elementType;

			if (ReflectionHelper.IsPrimitive(type))
			{
				return null;
			}

			if (!(isArray = ReflectionHelper.IsEnumerableOrArrayType(type, out elementType)))
			{
				elementType = type;
			}

			var ti = elementType.GetTypeInfo();
			if (typeof(AugmenterWrapper).GetTypeInfo().IsAssignableFrom(ti))
			{
				isWrapper = true;
				elementType = ti.GenericTypeArguments[0];
			}

			return new TypeInfoWrapper(elementType, isArray, isWrapper);
		}
	}
}
