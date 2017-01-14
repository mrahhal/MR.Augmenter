namespace MR.Augmenter
{
	public class TestModel1
	{
		public int Id { get; set; } = 42;
		public string Foo { get; set; } = "foo";
		public string Some { get; set; } = "bar";
	}

	public class TestModelA
	{
		public int Id { get; set; } = 42;
	}

	public class TestModelB : TestModelA
	{
		public string Foo { get; set; } = "foo";
	}

	public class TestModelB2 : TestModelA
	{
		public TestModel1 Model1 { get; set; } = new TestModel1();
	}

	public class TestModelC : TestModelB
	{
		public string Bar { get; set; } = "bar";
	}

	public class TestModelC2 : TestModelB2
	{
		public string Bar { get; set; } = "bar";
	}

	public class TestModelWithNested
	{
		public int Id { get; set; } = 42;
		public TestModelNested Nested { get; set; } = new TestModelNested();
	}

	public class TestModelNested
	{
		public int Id { get; set; } = 43;
		public TestModelNestedNested Nested { get; set; } = new TestModelNestedNested();
	}

	public class TestModelNestedNested
	{
		public int Id { get; set; } = 44;
	}
}
