using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace MR.Augmenter
{
	public class AugmenterBaseTest : CommonTestHost
	{
		[Fact]
		public void Augment_AugmenterConfigurationNotBuild_BuildsIt()
		{
			var configration = new AugmenterConfiguration();

			MocksHelper.AugmenterBase(configration);

			configration.Built.Should().BeTrue();
		}

		[Fact]
		public void Augment_Null_ReturnsNull()
		{
			var fixture = MocksHelper.AugmenterBase(new AugmenterConfiguration());
			object model = null;

			var result = fixture.Augment(model);

			result.Should().BeNull();
		}

		[Fact]
		public void Augment_PicksUpBaseClasses()
		{
			var fixture = MocksHelper.AugmenterBase(ConfigureCommon());
			var model = new TestModelC();

			fixture.Augment(model);

			var context = fixture.Contexts.First();
			context.TypeConfigurations.Should()
				.HaveCount(3).And
				.OnlyContain(tc => tc.Type.GetTypeInfo().IsAssignableFrom(typeof(TestModelC)));
		}

		[Fact]
		public void Augment_TypeConfigurationsInCorrectOrder()
		{
			var fixture = MocksHelper.AugmenterBase(ConfigureCommon());
			var model = new TestModelC();

			fixture.Augment(model);

			var context = fixture.Contexts.First();
			var configurations = context.TypeConfigurations;
			configurations.ElementAt(0).Type.Should().Be(typeof(TestModelA));
			configurations.ElementAt(1).Type.Should().Be(typeof(TestModelB));
			configurations.ElementAt(2).Type.Should().Be(typeof(TestModelC));
		}

		[Fact]
		public void Augment_State_FallsThrough()
		{
			var fixture = MocksHelper.AugmenterBase(ConfigureCommon());
			var model = new TestModelC();
			var someValue = "bars";

			fixture.Augment(model, addState: state =>
			{
				state.Add("key", someValue);
			});

			fixture.Contexts.First().State["key"].Should().Be(someValue);
		}

		[Fact]
		public void Augment_ChecksComplexPropertiesAnyway()
		{
			var fixture = MocksHelper.AugmenterBase(ConfigureCommon());
			var model = new
			{
				Inner = new TestModelC()
			};

			fixture.Augment(model);

			fixture.Contexts.Should().HaveCount(1);
		}
	}
}
