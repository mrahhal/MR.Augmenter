using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MR.Augmenter
{
	public class AugmenterBaseTest : TestHost
	{
		public class AugmentTest : AugmenterBaseTest
		{
			[Fact]
			public void AugmenterConfigurationNotBuild_Throw()
			{
				var configration = new AugmenterConfiguration();

				Assert.ThrowsAny<Exception>(() =>
				{
					MocksHelper.AugmenterBase(configration);
				});
			}

			[Fact]
			public async Task Null_ReturnsNull()
			{
				var fixture = MocksHelper.AugmenterBase(CreateBuiltConfiguration());
				object model = null;

				var result = await fixture.AugmentAsync(model);

				result.Should().BeNull();
			}

			[Fact]
			public async Task State_FallsThrough()
			{
				var fixture = MocksHelper.AugmenterBase(CreateCommonConfiguration());
				var model = new TestModelC();
				var someValue = "bars";

				await fixture.AugmentAsync(model, addState: state =>
				{
					state.Add("key", someValue);
				});

				fixture.Contexts.First().State["key"].Should().Be(someValue);
			}

			[Fact]
			public async Task PicksUpGlobalState()
			{
				var configuration = CreateCommonConfiguration();
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

				await fixture.AugmentAsync(new TestModel1());

				var context = fixture.Contexts.First();
				context.State["Foo"].Should().Be("foo");
			}

			[Fact]
			public async Task CopiesGlobalState()
			{
				var configuration = CreateCommonConfiguration();
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

				await fixture.AugmentAsync(new TestModel1(), addState: state =>
				{
					state["Some"] = "some";
				});

				var context = fixture.Contexts.Last();
				context.State["Foo"].Should().Be("foo");
				context.State["Some"].Should().Be("some");

				await fixture.AugmentAsync(new TestModel1());

				context = fixture.Contexts.Last();
				context.State["Foo"].Should().Be("foo");
				context.State.Should().NotContain("Some");
			}
		}

		private class SomeService
		{
			public string Foo { get; set; } = "foo";
		}
	}
}
