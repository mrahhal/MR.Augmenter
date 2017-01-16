using System;

namespace MR.Augmenter.Internal
{
	public class TypeInfoWrapper
	{
		public TypeInfoWrapper(Type type, bool isArray, bool isWrapper, bool isPrimitive)
		{
			Type = type;
			IsArray = isArray;
			IsWrapper = isWrapper;
			IsPrimitive = isPrimitive;
		}

		public Type Type { get; }

		public bool IsArray { get; }

		public bool IsWrapper { get; }

		public bool IsPrimitive { get; }
	}
}
