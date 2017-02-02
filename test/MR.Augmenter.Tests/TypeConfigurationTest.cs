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

		[Fact]
		public void ConfigureNested()
		{
			var tc = new TypeConfiguration<TestModelWithNested>();

			tc.ConfigureNested(x => x.Nested, ntc =>
			{
				ntc.SetAddState((x, s1, s2) =>
				{
					s2["ParentId"] = x.Id;
				});
			});

			tc.NestedConfigurations.Value.Should().HaveCount(1);
			tc.NestedConfigurations.Value.First().Invoking(c =>
			{
				c.Key.Name.Should().Be(nameof(TestModelWithNested.Nested));
				c.Value.Should().NotBeNull();
			});
		}

		[Fact]
		public void ConfigureNestedArray()
		{
			var tc = new TypeConfiguration<TestModelWithNestedArray>();

			tc.ConfigureNested(x => x.NestedArray, ntc =>
			{
				ntc.SetAddState((x, s1, s2) =>
				{
					s2["ParentId"] = x.Id;
				});
			});

			tc.NestedConfigurations.Value.Should().HaveCount(1);
			tc.NestedConfigurations.Value.First().Invoking(c =>
			{
				c.Key.Name.Should().Be(nameof(TestModelWithNested.Nested));
				c.Value.Should().NotBeNull();
			});
		}

		[Fact]
		public void ConfigureNested_AddState()
		{
			var tc = new TypeConfiguration<TestModelWithNested>();

			tc.ConfigureNested(x => x.Nested, ntc =>
			{
				ntc.SetAddState((x, s1, s2) =>
				{
					s2["ParentId"] = x.Id;
				});
			});

			var nested = tc.NestedConfigurations.Value.First();
			var state1 = new State();
			var state2 = new State();
			var model = new TestModelWithNested();
			nested.Value.AddState(model, state1, state2);

			state2["ParentId"].Should().Be(model.Id);
		}
	}
}
