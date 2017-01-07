using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace MR.Augmenter
{
	public class AugmenterBaseTest : CommonTestHost
	{
		private AugmenterConfiguration _configuration;
		private FakeAugmenterBase _fixture;

		public AugmenterBaseTest()
		{
			_configuration = ConfigureCommon();
			_fixture = new FakeAugmenterBase(_configuration);
		}

		[Fact]
		public void Augment_AugmenterConfigurationNotBuild_BuildsIt()
		{
			var configration = new AugmenterConfiguration();

			new FakeAugmenterBase(configration);

			configration.Built.Should().BeTrue();
		}

		[Fact]
		public void Augment_Null_ReturnsNull()
		{
			object model = null;

			var result = _fixture.Augment(model);

			result.Should().BeNull();
		}

		[Fact]
		public void Augment_PicksUpBaseClasses()
		{
			var model = CreateModelC();

			_fixture.Augment(model);

			var context = _fixture.Contexts.First();
			context.TypeConfigurations.Should()
				.HaveCount(3).And
				.OnlyContain(tc => tc.Type.GetTypeInfo().IsAssignableFrom(typeof(TestModelC)));
		}

		[Fact]
		public void Augment_TypeConfigurationsInCorrectOrder()
		{
			var model = CreateModelC();

			_fixture.Augment(model);

			var context = _fixture.Contexts.First();
			var configurations = context.TypeConfigurations;
			configurations.ElementAt(0).Type.Should().Be(typeof(TestModelA));
			configurations.ElementAt(1).Type.Should().Be(typeof(TestModelB));
			configurations.ElementAt(2).Type.Should().Be(typeof(TestModelC));
		}

		private TestModelC CreateModelC()
		{
			return new TestModelC();
		}
	}
}
