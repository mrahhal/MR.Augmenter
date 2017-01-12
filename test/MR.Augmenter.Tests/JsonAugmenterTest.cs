using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MR.Augmenter
{
	public class JsonAugmenterTest : CommonTestHost
	{
		private AugmenterConfiguration _configuration;
		private JsonAugmenter _fixture;

		public JsonAugmenterTest()
		{
			_configuration = new AugmenterConfiguration();
			_fixture = new JsonAugmenter(_configuration);
		}

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
					c.ConfigureAdd("Baz", (_, __) => 2);
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
					c.ConfigureAdd("Some", (_, __) => AugmentationValue.Ignore);
				}) as JObject;

				result["Some"].Should().BeNull();
			}

			[Fact]
			public void Remove_WithIgnore_IgnoresAugment()
			{
				var model = new TestModel1();

				var result = _fixture.Augment(model, c =>
				{
					c.ConfigureRemove(nameof(TestModel1.Foo), (_, __) => AugmentationValue.Ignore);
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
						c.ConfigureAdd("Foo", (_, __) => "42");
					});
					_configuration.Configure<TestModelNested>(c =>
					{
						c.ConfigureAdd("Foo", (_, __) => "43");
					});
					_configuration.Configure<TestModelNestedNested>(c =>
					{
						c.ConfigureAdd("Foo", (_, __) => "44");
					});

					_fixture = new JsonAugmenter(_configuration);
				}

				[Fact]
				public void Basic()
				{
					var model = new TestModelWithNested();

					var result = _fixture.Augment(model) as JObject;

					result.Value<string>("Foo").Should().Be("42");
					var nested = result.Value<JObject>("Nested");
					nested.Value<string>("Foo").Should().Be("43");
					var nestedNested = nested.Value<JObject>("Nested");
					nestedNested.Value<string>("Foo").Should().Be("44");
				}
			}

			public class State : JsonAugmenterTest
			{
				[Fact]
				public void Globally()
				{
					var model = new TestModel1();

					_configuration = new AugmenterConfiguration();
					_configuration.Configure<TestModel1>(c =>
					{
						c.ConfigureAdd("Bar", (x, state) =>
						{
							return state["key"];
						});
					});
					_fixture = new JsonAugmenter(_configuration);

					var result = _fixture.Augment(model, addState: state =>
					{
						state.Add("key", "bar");
					}) as JObject;

					result.Value<string>("Bar").Should().Be("bar");
				}

				[Fact]
				public void Locally()
				{
					var model = new TestModel1();

					var result = _fixture.Augment(model, c =>
					{
						c.ConfigureAdd("Bar", (x, state) =>
						{
							return state["key"];
						});
					}, state =>
					{
						state.Add("key", "bar");
					}) as JObject;

					result.Value<string>("Bar").Should().Be("bar");
				}

				[Fact]
				public void Nested()
				{
					var model = new TestModelWithNested();

					_configuration = new AugmenterConfiguration();
					_configuration.Configure<TestModelWithNested>(null);
					_configuration.Configure<TestModelNested>(c =>
					{
						c.ConfigureAdd("Foo", (x, state) =>
						{
							return state["key"];
						});
					});
					_fixture = new JsonAugmenter(_configuration);

					var result = _fixture.Augment(model, addState: state =>
					{
						state.Add("key", "foo");
					}) as JObject;

					result.Value<JObject>("Nested").Value<string>("Foo").Should().Be("foo");
				}

				[Fact]
				public void WithIgnore()
				{
					var model = new TestModel1();

					var result = _fixture.Augment(model, config =>
					{
						config.ConfigureAdd("Bar", (x, state) =>
						{
							if ((bool)state["key"])
							{
								return "YES";
							}
							return AugmentationValue.Ignore;
						});
					}, state =>
					{
						state.Add("key", false);
					}) as JObject;

					result["Bar"].Should().BeNull();
				}
			}
		}
	}
}
