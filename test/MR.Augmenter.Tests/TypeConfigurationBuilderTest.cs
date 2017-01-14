using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace MR.Augmenter
{
	public class TypeConfigurationBuilderTest
	{
		[Fact]
		public void TypeConfigurationAndTypeNull_Throws()
		{
			var list = new List<TypeConfiguration>();
			var builder = new TypeConfigurationBuilder(list);

			Assert.Throws<ArgumentNullException>(() =>
			{
				builder.Build(null, null);
			});
		}

		[Fact]
		public void TypeIsDifferentThanTypeConfigurationType_Throws()
		{
			var model1TypeConfiguration = new TypeConfiguration(typeof(TestModel1));
			var list = new List<TypeConfiguration>
			{
				model1TypeConfiguration
			};
			var builder = new TypeConfigurationBuilder(list);

			Assert.Throws<ArgumentException>(() =>
			{
				builder.Build(model1TypeConfiguration, typeof(TestModelA));
			});
		}

		[Fact]
		public void Unknown_ReturnsNull()
		{
			var list = new List<TypeConfiguration>();
			var builder = new TypeConfigurationBuilder(list);
			var anon = new { Foo = "foo" };

			var result = builder.Build(null, anon.GetType());

			result.Should().BeNull();
		}

		[Fact]
		public void Known()
		{
			var model1TypeConfiguration = new TypeConfiguration(typeof(TestModel1));
			var list = new List<TypeConfiguration>
			{
				model1TypeConfiguration
			};
			var builder = new TypeConfigurationBuilder(list);

			var result = builder.Build(model1TypeConfiguration, typeof(TestModel1));

			result.Should().Be(model1TypeConfiguration);
		}

		[Fact]
		public void EnsureConfigurationIsBuiltOnlyOnce()
		{
			var modelATypeConfiguration = new TypeConfiguration<TestModelA>();
			modelATypeConfiguration.ConfigureAdd("Some", (x, state) => "some");
			var modelBTypeConfiguration = new TypeConfiguration(typeof(TestModelB));
			var list = new List<TypeConfiguration>
			{
				modelATypeConfiguration,
				modelBTypeConfiguration
			};
			var builder = new TypeConfigurationBuilder(list);
			var result = builder.Build(modelBTypeConfiguration, typeof(TestModelB));

			var anon = new { Model = new TestModelB() };
			result = builder.Build(null, anon.GetType());

			modelBTypeConfiguration.BaseTypeConfigurations.Should().HaveCount(1);
		}

		public class Step1Test : TypeConfigurationBuilderTest
		{
			[Fact]
			public void CollectsBaseTypes()
			{
				var modelATypeConfiguration = new TypeConfiguration(typeof(TestModelA));
				var modelBTypeConfiguration = new TypeConfiguration(typeof(TestModelB));
				var modelCTypeConfiguration = new TypeConfiguration(typeof(TestModelC));
				var list = new List<TypeConfiguration>
				{
					modelATypeConfiguration,
					modelBTypeConfiguration,
					modelCTypeConfiguration
				};
				var builder = new TypeConfigurationBuilder(list);

				var result = builder.Build(modelCTypeConfiguration, typeof(TestModelC));

				var baseTypeConfigurations = result.BaseTypeConfigurations.Should().HaveCount(2).And.Subject;
				baseTypeConfigurations.ElementAt(0).Type.Should().Be(typeof(TestModelA));
				baseTypeConfigurations.ElementAt(1).Type.Should().Be(typeof(TestModelB));
			}

			[Fact]
			public void CollectsBaseTypes_Discontinuous()
			{
				var modelATypeConfiguration = new TypeConfiguration(typeof(TestModelA));
				var modelCTypeConfiguration = new TypeConfiguration(typeof(TestModelC));
				var list = new List<TypeConfiguration>
				{
					modelATypeConfiguration,
					modelCTypeConfiguration
				};
				var builder = new TypeConfigurationBuilder(list);

				var result = builder.Build(modelCTypeConfiguration, typeof(TestModelC));

				var baseTypeConfigurations = result.BaseTypeConfigurations.Should().HaveCount(2).And.Subject;
				baseTypeConfigurations.ElementAt(0).Type.Should().Be(typeof(TestModelA));
				baseTypeConfigurations.ElementAt(1).Type.Should().Be(typeof(TestModelB));
			}

			[Fact]
			public void CollectsBaseTypes_Discontinuous_ButWithNested()
			{
				var modelATypeConfiguration = new TypeConfiguration(typeof(TestModelA));
				var modelCTypeConfiguration = new TypeConfiguration(typeof(TestModelC2));
				var list = new List<TypeConfiguration>
				{
					modelATypeConfiguration,
					modelCTypeConfiguration
				};
				var builder = new TypeConfigurationBuilder(list);

				var result = builder.Build(modelCTypeConfiguration, typeof(TestModelC2));

				var baseTypeConfigurations = result.BaseTypeConfigurations.Should().HaveCount(2).And.Subject;
				baseTypeConfigurations.ElementAt(0).Type.Should().Be(typeof(TestModelA));
				baseTypeConfigurations.ElementAt(1).Type.Should().Be(typeof(TestModelB2));
			}

			[Fact]
			public void CollectsNestedTypes()
			{
				var modelWithNestedTypeConfiguration = new TypeConfiguration(typeof(TestModelWithNested));
				var modelNestedTypeConfiguration = new TypeConfiguration(typeof(TestModelNested));
				var modelNestedNestedTypeConfiguration = new TypeConfiguration(typeof(TestModelNestedNested));
				var list = new List<TypeConfiguration>
				{
					modelWithNestedTypeConfiguration,
					modelNestedTypeConfiguration,
					modelNestedNestedTypeConfiguration
				};
				var builder = new TypeConfigurationBuilder(list);

				var result = builder.Build(modelWithNestedTypeConfiguration, typeof(TestModelWithNested));

				var nestedResult = result.NestedTypeConfigurations.Should().HaveCount(1).And.Subject.First();
				nestedResult.Invoking(p =>
				{
					p.Key.Should().BeOfType(typeof(TestModelNested));
					p.Value.Should().Be(modelNestedTypeConfiguration);
				});
				var nestedNestedResult = nestedResult.Value.TypeConfiguration.NestedTypeConfigurations.Should().HaveCount(1)
					.And.Subject.First();
				nestedNestedResult.Invoking(p =>
				{
					p.Key.Should().BeOfType(typeof(TestModelNestedNested));
					p.Value.Should().Be(modelNestedNestedTypeConfiguration);
				});
			}
		}

		public class Step2Test : TypeConfigurationBuilderTest
		{
			[Fact]
			public void Unknown_WithNested()
			{
				var modelATypeConfiguration = new TypeConfiguration(typeof(TestModelA));
				var list = new List<TypeConfiguration>
				{
					modelATypeConfiguration
				};
				var builder = new TypeConfigurationBuilder(list);
				var anon = new { Model = new TestModelA() };

				var result = builder.Build(null, anon.GetType());

				result.Should().NotBeNull();
				result.NestedTypeConfigurations.First().Invoking(p =>
				{
					p.Key.Should().BeOfType(typeof(TestModelA));
					p.Value.Should().Be(modelATypeConfiguration);
				});
			}

			[Fact]
			public void NestedEnumerable()
			{
				var modelATypeConfiguration = new TypeConfiguration(typeof(TestModelA));
				var list = new List<TypeConfiguration>()
				{
					modelATypeConfiguration
				};
				var builder = new TypeConfigurationBuilder(list);
				var anon = new { Models = new[] { new TestModelA(), new TestModelA() } };

				var result = builder.Build(null, anon.GetType());

				result.Should().NotBeNull();
				result.NestedTypeConfigurations.First().Invoking(p =>
				{
					p.Value.Kind.Should().Be(NestedTypeConfigurationKind.Array);
				});
			}
		}
	}
}
