using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
					c.ConfigureAdd("Bar", (_, __) => "bar");
				});
				config.Build();
			});

			var provider = services.BuildServiceProvider();
			var augmenter = provider.GetRequiredService<IAugmenter>();
			var configuration = provider.GetRequiredService<AugmenterConfiguration>();

			configuration.TypeConfigurations.Should()
				.HaveCount(1).And
				.Subject.First().Type.Should().Be(typeof(TestModel1));
		}
	}
}
