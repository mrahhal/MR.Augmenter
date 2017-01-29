using System;
using System.Reflection;
using MR.Augmenter.Internal;

namespace MR.Augmenter
{
	public class APropertyInfo
	{
		public APropertyInfo(PropertyInfo pi, TypeInfoWrapper tiw, TypeConfiguration tc)
		{
			PropertyInfo = pi;
			TypeInfoWrapper = tiw;
			TypeConfiguration = tc;
			GetValueFunc = PropertyHelper.MakeFastPropertyGetter(pi);
		}

		public PropertyInfo PropertyInfo { get; }

		public TypeInfoWrapper TypeInfoWrapper { get; }

		public Type Type => TypeInfoWrapper.Type;

		// Can be null to signify that this property doesn't need to be touched.
		public TypeConfiguration TypeConfiguration { get; }

		// Compiled from an expression
		public Func<object, object> GetValueFunc { get; }

		public object GetValue(object instance)
		{
			return GetValueFunc(instance);
		}
	}
}
