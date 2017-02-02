using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using AArray = System.Collections.Generic.List<object>;
using AObject = System.Collections.Generic.Dictionary<string, object>;

namespace MR.Augmenter
{
	public class AugmenterTest : TestHost
	{
		private AugmenterConfiguration _configuration;
		private Augmenter _fixture;

		public AugmenterTest()
		{
			_configuration = CreateCommonConfiguration();
			_fixture = MocksHelper.Augmenter(_configuration);
		}

		public class BasicTest : AugmenterTest
		{
			[Fact]
			public async Task Basic()
			{
				var model = new TestModel1();

				var result = await _fixture.AugmentAsync(model) as AObject;

				((string)result["Bar"]).Should().Be($"({model.Id})");
				result.Should().NotContainKey(nameof(TestModel1.Some));
			}

			[Fact]
			public async Task WithLocal()
			{
				var model = new TestModel1();

				var result = await _fixture.AugmentAsync(model, c =>
				{
					c.Add("Baz", (_, __) => 2);
				}) as AObject;

				result["Bar"].Cast<string>().Should().Be($"({model.Id})");
				result["Baz"].Cast<int>().Should().Be(2);
				result.Should().NotContainKey(nameof(TestModel1.Some));
			}

			[Fact]
			public async Task Add_WithIgnore_IgnoresAugment()
			{
				var model = new TestModel1();

				var result = await _fixture.AugmentAsync(model, c =>
				{
					c.Add("Some", (_, __) => AugmentationValue.Ignore);
				}) as AObject;

				result.Should().NotContainKey("Some");
			}

			[Fact]
			public async Task Remove_WithIgnore_IgnoresAugment()
			{
				var model = new TestModel1();

				var result = await _fixture.AugmentAsync(model, c =>
				{
					c.Remove(nameof(TestModel1.Foo), (_, __) => AugmentationValue.Ignore);
				}) as AObject;

				result[nameof(TestModel1.Foo)].Cast<string>().Should().Be("foo");
			}

			[Fact]
			public async Task Enums()
			{
				var model = new TestModelWithEnum();

				var result = await _fixture.AugmentAsync(model, c => { /* force augment */ }) as AObject;

				result[nameof(TestModelWithEnum.Some)].Cast<SomeEnum>().Should().Be(SomeEnum.Bar);
			}

			[Fact]
			public async Task WrapperAroundArray()
			{
				var wrapper = new AugmenterWrapper<TestModelForWrapping>(new[]
				{
					new TestModelForWrapping() { Id = 42, Model = new TestModel1() },
					new TestModelForWrapping() { Id = 43, Model = new TestModel1() }
				});
				wrapper.SetConfiguration(c =>
				{
					c.Add("Foo", (x, state) => $"{x.Id}-foo");
				});
				var model = new
				{
					Models = wrapper
				};

				var result = await _fixture.AugmentAsync(model) as AObject;

				result["Models"].GetType().Should().Be(typeof(AArray));
				var list = result["Models"] as AArray;
				list.Should().HaveCount(2);
				list[0].Cast<AObject>()["Foo"].Cast<string>().Should().Be("42-foo");
				list[1].Cast<AObject>()["Foo"].Cast<string>().Should().Be("43-foo");
			}
		}

		public class NestedTest : AugmenterTest
		{
			public NestedTest()
			{
				_configuration = new AugmenterConfiguration();
				_configuration.Configure<TestModel1>(c => { });
				_configuration.Configure<TestModelWithNested>(c =>
				{
					c.Add("Foo", (_, __) => "42");
				});
				_configuration.Configure<TestModelNested>(c =>
				{
					c.Add("Foo", (_, __) => "43");
				});
				_configuration.Configure<TestModelNestedNested>(c =>
				{
					c.Add("Foo", (_, __) => "44");
				});

				_configuration.Build();
				_fixture = MocksHelper.Augmenter(_configuration);
			}

			[Fact]
			public async Task Basic()
			{
				var model = new TestModelWithNested();

				var result = await _fixture.AugmentAsync(model) as AObject;

				result["Foo"].Cast<string>().Should().Be("42");
				var nested = result["Nested"].Cast<AObject>();

				nested["Foo"].Cast<string>().Should().Be("43");
				var nestedNested = nested["Nested"].Cast<AObject>();
				nestedNested["Foo"].Cast<string>().Should().Be("44");
			}

			[Fact]
			public async Task Array()
			{
				var model = new { Models = new[] { new TestModelWithNested(), new TestModelWithNested() } };

				var result = await _fixture.AugmentAsync(model) as AObject;

				result["Models"].GetType().Should().Be(typeof(AArray));
				result["Models"].Cast<AArray>().Should().HaveCount(2);
			}

			[Fact]
			public async Task HandlesNullComplex()
			{
				var model = new TestModelForWrapping() { Id = 42, Model = null };

				var result = await _fixture.AugmentAsync(model) as AObject;

				result["Model"].Should().BeNull();
			}

			[Fact]
			public async Task Wrapper()
			{
				var model = new
				{
					Model = new AugmenterWrapper<TestModelForWrapping>(
						new TestModelForWrapping() { Id = 42, Model = new TestModel1() })
				};

				var result = await _fixture.AugmentAsync(model) as AObject;

				result["Model"].Cast<AObject>().Should().HaveCount(3);
				result["Model"].Cast<AObject>()["Id"].Cast<int>().Should().Be(42);
				result["Model"].Cast<AObject>()["Model"].Cast<AObject>()["Id"].Cast<int>().Should().Be(42);
			}

			[Fact]
			public async Task ArrayAndWrapper()
			{
				var model = new
				{
					Models = new[]
					{
						new AugmenterWrapper<TestModelForWrapping>(
							new TestModelForWrapping() { Id = 42, Model = new TestModel1() }),
						new AugmenterWrapper<TestModelForWrapping>(
							new TestModelForWrapping() { Id = 43, Model = new TestModel1() })
					}
				};

				var result = await _fixture.AugmentAsync(model) as AObject;

				result["Models"].GetType().Should().Be(typeof(AArray));
				var list = result["Models"] as AArray;
				list.Should().HaveCount(2);
				list[0].Cast<AObject>()["Id"].Cast<int>().Should().Be(42);
				list[1].Cast<AObject>()["Id"].Cast<int>().Should().Be(43);
			}
		}

		public class StateTest : AugmenterTest
		{
			public StateTest()
			{
				_configuration = CreateBuiltConfiguration();
				_fixture = MocksHelper.Augmenter(_configuration);
			}

			[Fact]
			public async Task Globally()
			{
				var model = new TestModel1();

				_configuration = new AugmenterConfiguration();
				_configuration.Configure<TestModel1>(c =>
				{
					c.Add("Bar", (x, state) =>
					{
						return state["key"];
					});
				});
				_configuration.Build();
				_fixture = MocksHelper.Augmenter(_configuration);

				var result = await _fixture.AugmentAsync(model, addState: state =>
				{
					state.Add("key", "bar");
				}) as AObject;

				result["Bar"].Cast<string>().Should().Be("bar");
			}

			[Fact]
			public async Task Locally()
			{
				var model = new TestModel1();

				var result = await _fixture.AugmentAsync(model, c =>
				{
					c.Add("Bar", (x, state) =>
					{
						return state["key"];
					});
				}, state =>
				{
					state.Add("key", "bar");
				}) as AObject;

				result["Bar"].Cast<string>().Should().Be("bar");
			}

			[Fact]
			public async Task Nested()
			{
				var model = new TestModelWithNested();

				_configuration = new AugmenterConfiguration();
				_configuration.Configure<TestModelNested>(c =>
				{
					c.Add("Foo", (x, state) =>
					{
						return state["key"];
					});
				});
				_configuration.Build();
				_fixture = MocksHelper.Augmenter(_configuration);

				var result = await _fixture.AugmentAsync(model, addState: state =>
				{
					state.Add("key", "foo");
				}) as AObject;

				result["Nested"].Cast<AObject>()["Foo"].Cast<string>().Should().Be("foo");
			}

			[Fact]
			public async Task WithIgnore()
			{
				var model = new TestModel1();

				var result = await _fixture.AugmentAsync(model, config =>
				{
					config.Add("Bar", (x, state) =>
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
				}) as AObject;

				result.Should().NotContainKey("Bar");
			}
		}

		public class NestedConfigureTest : AugmenterTest
		{
			public NestedConfigureTest()
			{
				var configuration = new AugmenterConfiguration();

				configuration.Configure<NestedConfigureModels.Model1>(c =>
				{
					c.Add("Bar", (x, _) => $"({x.Id})");
					c.Remove(nameof(NestedConfigureModels.Model1.Ex));
					c.ConfigureNested(x => x.Nested1, nc =>
					{
						nc.SetAddState((m, s1, s2) =>
						{
							s2["ParentId"] = m.Id;
						});

						nc.SetTypeConfiguration(ntc =>
						{
							ntc.Add("Some", (x, state) => "some");
						});
					});
				});
				configuration.Configure<NestedConfigureModels.Model2>(c =>
				{
				});
				configuration.Configure<NestedConfigureModels.Model3>(c =>
				{
					c.ConfigureNestedArray(x => x.Nested, nc =>
					{
						nc.SetAddState((m, s1, s2) =>
						{
							s2["ParentId"] = m.Id;
						});

						nc.SetTypeConfiguration(ntc =>
						{
							ntc.Add("Some", (x, state) => "some");
						});
					});
				});
				configuration.Configure<NestedConfigureModels.Model4>(c =>
				{
					c.ConfigureNestedArray(x => x.Nested, nc =>
					{
						nc.SetAddState((m, s1, s2) =>
						{
							s2["ParentId"] = m.Id;
						});

						nc.SetTypeConfiguration(ntc =>
						{
							ntc.Add("Some", (x, state) => "some");
						});
					});
				});
				configuration.Configure<NestedConfigureModels.ModelNested>(c =>
				{
					c.AddIf("ParentId", "ParentId", (m, s) => s["ParentId"]);
				});

				configuration.Build();
				_configuration = configuration;
				_fixture = MocksHelper.Augmenter(_configuration);
			}

			[Fact]
			public async Task Picks_up_nested_state_and_configuration()
			{
				var model = new NestedConfigureModels.Model2();

				var result = await _fixture.AugmentAsync(model) as AObject;

				var nested = result["Nested1"].Cast<AObject>();
				nested["ParentId"].Cast<int>().Should().Be(model.Id);
				nested["Some"].Cast<string>().Should().Be("some");
			}

			[Fact]
			public async Task Other_entities_of_the_same_type_are_not_affected()
			{
				var model = new NestedConfigureModels.Model2();

				var result = await _fixture.AugmentAsync(model) as AObject;

				var nested = result["Nested2"].Cast<AObject>();
				nested.Should().NotContainKeys("ParentId", "Some");
			}

			[Fact]
			public async Task Works_with_enumerables()
			{
				var model = new NestedConfigureModels.Model3();

				var result = await _fixture.AugmentAsync(model) as AObject;

				var nested = result["Nested"].Cast<AArray>();
				foreach (var item in nested)
				{
					item.Cast<AObject>()["ParentId"].Should().Be(model.Id);
					item.Cast<AObject>()["Some"].Should().Be("some");
				}
			}

			[Fact]
			public async Task Works_with_arrays()
			{
				var model = new NestedConfigureModels.Model4();

				var result = await _fixture.AugmentAsync(model) as AObject;

				var nested = result["Nested"].Cast<AArray>();
				foreach (var item in nested)
				{
					item.Cast<AObject>()["ParentId"].Should().Be(model.Id);
					item.Cast<AObject>()["Some"].Should().Be("some");
				}
			}
		}
	}
}
