using System;

namespace MR.Augmenter.Internal
{
	public class TypeInfoWrapper
	{
		public TypeInfoWrapper(Type type, bool isArray, bool isWrapper)
		{
			Type = type;
			IsArray = isArray;
			IsWrapper = isWrapper;
		}

		public Type Type { get; set; }

		public bool IsArray { get; set; }

		public bool IsWrapper { get; set; }
	}
}
