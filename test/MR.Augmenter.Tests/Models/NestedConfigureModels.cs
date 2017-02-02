using System.Collections.Generic;

namespace MR.Augmenter
{
	public static class NestedConfigureModels
	{
		public class Model1
		{
			public int Id { get; set; } = 42;
			public string Ex { get; set; } = "ex";
			public ModelNested Nested1 { get; set; } = new ModelNested();
		}

		public class Model2 : Model1
		{
			public string Foo { get; set; } = "foo";
			public ModelNested Nested2 { get; set; } = new ModelNested();
		}

		public class Model3
		{
			public int Id { get; set; } = 42;
			public List<ModelNested> Nested { get; set; } = new List<ModelNested> { new ModelNested(), new ModelNested() };
		}

		public class Model4
		{
			public int Id { get; set; } = 42;
			public ModelNested[] Nested { get; set; } = new[] { new ModelNested(), new ModelNested() };
		}

		public class ModelNested
		{
			public int Id { get; set; } = 43;
			public string Secret { get; set; } = "secret";
		}
	}
}
