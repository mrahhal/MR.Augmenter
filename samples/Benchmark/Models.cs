using System;

namespace Benchmark
{
	public class ModelA
	{
		public int Id { get; set; } = 42;
		public string Bar { get; set; } = "bar";
		public string Some { get; set; } = "some";
		public string Secret { get; set; } = Guid.NewGuid().ToString("N");
	}

	public class ModelWithNested : ModelA
	{
		public ModelNested Nested { get; set; } = new ModelNested();
		public string Foo { get; set; } = $"{nameof(ModelWithNested)}-Foo";
	}

	public class ModelNested
	{
		public int Id { get; set; } = 43;

		public string Foo { get; set; } = $"{nameof(ModelNested)}-Foo";
	}
}
