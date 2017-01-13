using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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

		[Fact]
		public void Augment_PicksUpGlobalState()
		{
			var configuration = ConfigureCommon();
			configuration.ConfigureGlobalState = (state, provider) =>
			{
				var someService = provider.GetService<SomeService>();
				state["Foo"] = someService.Foo;
				return Task.CompletedTask;
			};
			var services = new ServiceCollection();
			services.AddSingleton(configuration);
			services.AddSingleton<FakeAugmenterBase>();
			services.AddSingleton<SomeService>();
			var p = services.BuildServiceProvider();
			var fixture = MocksHelper.For<FakeAugmenterBase>(p);

			fixture.Augment(new TestModel1());

			var context = fixture.Contexts.First();
			context.State["Foo"].Should().Be("foo");
		}

		[Fact]
		public void Augment_CopiesGlobalState()
		{
			var configuration = ConfigureCommon();
			configuration.ConfigureGlobalState = (state, provider) =>
			{
				var someService = provider.GetService<SomeService>();
				state["Foo"] = someService.Foo;
				return Task.CompletedTask;
			};
			var services = new ServiceCollection();
			services.AddSingleton(configuration);
			services.AddSingleton<FakeAugmenterBase>();
			services.AddSingleton<SomeService>();
			var p = services.BuildServiceProvider();
			var fixture = MocksHelper.For<FakeAugmenterBase>(p);

			fixture.Augment(new TestModel1(), addState: state =>
			{
				state["Some"] = "some";
			});

			var context = fixture.Contexts.Last();
			context.State["Foo"].Should().Be("foo");
			context.State["Some"].Should().Be("some");

			fixture.Augment(new TestModel1());

			context = fixture.Contexts.Last();
			context.State["Foo"].Should().Be("foo");
			context.State.Should().NotContain("Some");
		}

		private class SomeService
		{
			public string Foo { get; set; } = "foo";
		}
	}
}
