using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace MR.Augmenter
{
	public class AugmenterServiceCollectionExtensionsTest : TestHost
	{
		[Fact]
		public void Works()
		{
			var services = new ServiceCollection();
			services.AddAugmenter(config =>
			{
				config.Configure<TestModel1>(c =>
				{
					c.Add("Bar", (_, __) => "bar");
				});
			});

			services.Configure<AugmenterConfiguration>(config =>
			{
				config.Configure<TestModel1>(c =>
				{
					c.Add("Bar2", (_, __) => "bar2");
				});
			});

			var provider = services.BuildServiceProvider();
			var augmenter = provider.GetRequiredService<IAugmenter>();
			var configuration = provider.GetRequiredService<IOptions<AugmenterConfiguration>>().Value;

			configuration.TypeConfigurations.Should()
				.HaveCount(1).And
				.Subject.First().Type.Should().Be(typeof(TestModel1));
			configuration.TypeConfigurations.First().Augments.Should().HaveCount(2);
		}
	}
}
