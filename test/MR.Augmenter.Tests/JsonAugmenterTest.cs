using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MR.Augmenter
{
	public class JsonAugmenterTest : CommonTestHost
	{
		private AugmenterConfiguration _configuration;
		private JsonAugmenter _fixture;

		public class AugmentMethod : JsonAugmenterTest
		{
			public AugmentMethod()
			{
				_configuration = ConfigureCommon();
				_fixture = new JsonAugmenter(_configuration);
			}

			[Fact]
			public void Basic()
			{
				var model = new TestModel1();

				var result = _fixture.Augment(model) as JObject;

				result["Bar"].Value<string>().Should().Be($"({model.Id})");
				result[nameof(TestModel1.Some)].Should().BeNull();
			}

			[Fact]
			public void Basic_WithLocal()
			{
				var model = new TestModel1();

				var result = _fixture.Augment(model, c =>
				{
					c.ConfigureAdd("Baz", _ => 2);
				}) as JObject;

				result["Bar"].Value<string>().Should().Be($"({model.Id})");
				result["Baz"].Value<int>().Should().Be(2);
				result[nameof(TestModel1.Some)].Should().BeNull();
			}

			[Fact]
			public void Add_WithIgnore_IgnoresAugment()
			{
				var model = new TestModel1();

				var result = _fixture.Augment(model, c =>
				{
					c.ConfigureAdd("Some", _ => AugmentationValue.Ignore);
				}) as JObject;

				result["Some"].Should().BeNull();
			}

			[Fact]
			public void Remove_WithIgnore_IgnoresAugment()
			{
				var model = new TestModel1();

				var result = _fixture.Augment(model, c =>
				{
					c.ConfigureRemove(nameof(TestModel1.Foo), _ => AugmentationValue.Ignore);
				}) as JObject;

				result.Value<string>(nameof(TestModel1.Foo)).Should().Be("foo");
			}

			public class Nested : JsonAugmenterTest
			{
				public Nested()
				{
					_configuration = new AugmenterConfiguration();
					_configuration.Configure<TestModelWithNested>(c =>
					{
						c.ConfigureAdd("Foo", _ => "42");
					});
					_configuration.Configure<TestModelNested>(c =>
					{
						c.ConfigureAdd("Foo", _ => "43");
					});
					_configuration.Configure<TestModelNestedNested>(c =>
					{
						c.ConfigureAdd("Foo", _ => "44");
					});

					_fixture = new JsonAugmenter(_configuration);
				}

				[Fact]
				public void Basic()
				{
					var model = new TestModelWithNested();

					var result = _fixture.Augment(model) as JObject;

					result.Value<string>("Foo").Should().Be("42");
					var nested = result["Nested"].Value<JObject>();
					nested.Value<string>("Foo").Should().Be("43");
					var nestedNested = nested["Nested"].Value<JObject>();
					nestedNested.Value<string>("Foo").Should().Be("44");
				}
			}
		}
	}
}
