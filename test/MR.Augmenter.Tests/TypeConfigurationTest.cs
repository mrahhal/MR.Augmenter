using System.Linq;
using FluentAssertions;
using Xunit;

namespace MR.Augmenter
{
	public class TypeConfigurationTest : TestHost
	{
		[Fact]
		public void AddIf_False()
		{
			var tc = new TypeConfiguration<TestModel1>();
			tc.AddIf("Foo", "IsAdmin", (x, s) => x.Id);

			var model = new TestModel1();
			var state = new State();
			state["IsAdmin"] = Boxed.False;
			var result = tc.Augments.First().ValueFunc(model, state);

			result.Should().Be(AugmentationValue.Ignore);
		}

		[Fact]
		public void AddIf_True()
		{
			var tc = new TypeConfiguration<TestModel1>();
			tc.AddIf("Foo", "IsAdmin", (x, s) => x.Id);

			var model = new TestModel1();
			var state = new State();
			state["IsAdmin"] = Boxed.True;
			var result = tc.Augments.First().ValueFunc(model, state);

			result.Should().Be(model.Id);
		}

		[Fact]
		public void Add_AddsAugmentToTypeConfiguration()
		{
			var tc = new TypeConfiguration<TestModel1>();

			tc.Add("Foo", (x, _) => $"({x.Id})");

			tc.Augments.First().Invoking(o =>
			{
				o.Name.Should().Be("Foo");
				o.Kind.Should().Be(AugmentKind.Add);
				o.ValueFunc.Should().NotBeNull();
			});
		}

		[Fact]
		public void ExposeIf_False()
		{
			var tc = new TypeConfiguration<TestModel1>();
			tc.ExposeIf("Foo", "IsAdmin");

			var model = new TestModel1();
			var state = new State();
			state["IsAdmin"] = Boxed.False;
			var result = tc.Augments.First().ValueFunc(model, state);

			result.Should().Be(null);
		}

		[Fact]
		public void ExposeIf_True()
		{
			var tc = new TypeConfiguration<TestModel1>();
			tc.ExposeIf("Foo", "IsAdmin");

			var model = new TestModel1();
			var state = new State();
			state["IsAdmin"] = Boxed.True;
			var result = tc.Augments.First().ValueFunc(model, state);

			result.Should().Be(AugmentationValue.Ignore);
		}

		[Fact]
		public void Remove_AddsAugmentToTypeConfiguration()
		{
			var tc = new TypeConfiguration<TestModel1>();

			tc.Remove("Foo");

			tc.Augments.First().Invoking(o =>
			{
				o.Name.Should().Be("Foo");
				o.Kind.Should().Be(AugmentKind.Remove);
				o.ValueFunc.Should().BeNull();
			});
		}
	}
}
