using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace MR.Augmenter
{
	public class AugmenterConfigurationTest : CommonTestHost
	{
		[Fact]
		public void ConfigureAdd_AddsTypeConfigration()
		{
			var configuration = Create();

			configuration.Configure<TestModel1>(c =>
			{
				c.ConfigureAdd("Foo", (x, _) => $"({x.Id})");
			});

			configuration.TypeConfigurations.Should()
				.HaveCount(1).And
				.Subject.First().Type.Should().Be(typeof(TestModel1));
		}

		[Fact]
		public void ConfigureRemove()
		{
			var configuration = Create();

			configuration.Configure<TestModel1>(c =>
			{
				c.ConfigureRemove("Foo");
			});

			configuration.TypeConfigurations.First().Augments.First().Invoking(o =>
			{
				o.Name.Should().Be("Foo");
				o.Kind.Should().Be(AugmentKind.Remove);
			});
		}

		[Fact]
		public void Build_PicksUpBaseClasses()
		{
			var configuration = ConfigureCommon();

			configuration.Build();

			var t = configuration.TypeConfigurations.FirstOrDefault(tc => tc.Type == typeof(TestModelC));
			t.BaseTypeConfigurations.Should()
				.HaveCount(2).And
				.OnlyContain(tc => tc.Type.GetTypeInfo().IsAssignableFrom(typeof(TestModelC)));
		}

		private AugmenterConfiguration Create() => new AugmenterConfiguration();
	}
}
