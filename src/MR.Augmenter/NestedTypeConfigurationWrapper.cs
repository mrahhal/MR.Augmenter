namespace MR.Augmenter
{
	public class NestedTypeConfigurationWrapper
	{
		public TypeConfiguration TypeConfiguration { get; private set; }

		public NestedTypeConfigurationKind Kind { get; private set; }

		public static NestedTypeConfigurationWrapper CreateObject(TypeConfiguration typeConfiguration)
		{
			return new NestedTypeConfigurationWrapper
			{
				TypeConfiguration = typeConfiguration,
				Kind = NestedTypeConfigurationKind.Object
			};
		}

		public static NestedTypeConfigurationWrapper CreateArray(TypeConfiguration typeConfiguration)
		{
			return new NestedTypeConfigurationWrapper
			{
				TypeConfiguration = typeConfiguration,
				Kind = NestedTypeConfigurationKind.Array
			};
		}
	}

	public enum NestedTypeConfigurationKind
	{
		Object,
		Array
	}
}
