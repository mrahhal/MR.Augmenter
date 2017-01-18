using System.Collections.Generic;

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

	public class TestModelForWrapping
	{
		public int Id { get; set; }
		public string Some { get; set; }
		public TestModel1 Model { get; set; }
	}

	public class TestModelWithNested
	{
		public int Id { get; set; } = 42;
		public string Some1 { get; set; } = "43";
		public string Some2 { get; set; } = "44";
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

	public class TestModelWithPrimitiveList
	{
		public List<string> Strings { get; set; } = new List<string>();
	}

	public class TestModelSelfReferencing
	{
		public int Id { get; set; } = 42;
		public TestModelSelfReferencing Model { get; set; }
	}

	public class TestModelSelfReferencingArray
	{
		public int Id { get; set; } = 42;
		public List<TestModelSelfReferencingArray> Children { get; set; }
	}

	public class TestModelWithStaticReference
	{
		public int Id { get; set; } = 42;
		public static TestModelWithStaticReference Instance { get; set; } = new TestModelWithStaticReference();
	}

	public class TestModelWithEnum
	{
		public int Id { get; set; } = 42;
		public SomeEnum Some { get; set; } = SomeEnum.Bar;
	}

	public enum SomeEnum
	{
		Foo,
		Bar
	}
}
