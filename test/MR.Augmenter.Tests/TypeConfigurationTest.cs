using System.Linq;
using FluentAssertions;
using Xunit;

namespace MR.Augmenter
{
	public class TypeConfigurationTest : TestHost
	{
		[Fact]
		public void ConfigureAdd_AddsAugmentToTypeConfiguration()
		{
			var tc = new TypeConfiguration<TestModel1>();

			tc.ConfigureAdd("Foo", b => $"({b.Id})");

			tc.Augments.First().Invoking(o =>
			{
				o.Name.Should().Be("Foo");
				o.Kind.Should().Be(AugmentKind.Add);
				o.ValueFunc.Should().NotBeNull();
			});
		}

		[Fact]
		public void ConfigureRemove_AddsAugmentToTypeConfiguration()
		{
			var tc = new TypeConfiguration<TestModel1>();

			tc.ConfigureRemove("Foo");

			tc.Augments.First().Invoking(o =>
			{
				o.Name.Should().Be("Foo");
				o.Kind.Should().Be(AugmentKind.Remove);
				o.ValueFunc.Should().BeNull();
			});
		}
	}
}
