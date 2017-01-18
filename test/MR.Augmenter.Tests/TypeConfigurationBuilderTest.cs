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
			var anon = new { Foo = "foo", Model = new TestModel1() };

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

		[Fact]
		public void DetectsSelfReferencing()
		{
			var modelTypeConfiguration = new TypeConfiguration<TestModelSelfReferencing>();
			modelTypeConfiguration.ConfigureAdd("Some", (x, state) => "some");
			var list = new List<TypeConfiguration>
			{
				modelTypeConfiguration
			};
			var builder = new TypeConfigurationBuilder(list);
			var result = builder.Build(modelTypeConfiguration, typeof(TestModelSelfReferencing));

			modelTypeConfiguration.Properties.Should().HaveCount(2);
			modelTypeConfiguration.Properties[1].TypeConfiguration.Should().Be(modelTypeConfiguration);
		}

		[Fact]
		public void DetectsSelfReferencingArray()
		{
			var modelTypeConfiguration = new TypeConfiguration<TestModelSelfReferencingArray>();
			modelTypeConfiguration.ConfigureAdd("Some", (x, state) => "some");
			var list = new List<TypeConfiguration>
			{
				modelTypeConfiguration
			};
			var builder = new TypeConfigurationBuilder(list);
			var result = builder.Build(modelTypeConfiguration, typeof(TestModelSelfReferencingArray));

			modelTypeConfiguration.Properties.Should().HaveCount(2);
			modelTypeConfiguration.Properties[1].TypeConfiguration.Should().Be(modelTypeConfiguration);
			modelTypeConfiguration.Properties[1].TypeInfoWrapper.IsArray.Should().BeTrue();
		}

		[Fact]
		public void IgnoresStaticReferences()
		{
			var modelTypeConfiguration = new TypeConfiguration<TestModelWithStaticReference>();
			modelTypeConfiguration.ConfigureAdd("Some", (x, state) => "some");
			var list = new List<TypeConfiguration>
			{
				modelTypeConfiguration
			};
			var builder = new TypeConfigurationBuilder(list);
			var result = builder.Build(modelTypeConfiguration, typeof(TestModelWithStaticReference));

			modelTypeConfiguration.Properties.Should().HaveCount(1);
		}

		public class Step1Test : TestHost
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
			public void CollectsBaseTypesRegardlessOfTheirConfiguration()
			{
				var modelCTypeConfiguration = new TypeConfiguration(typeof(TestModelC));
				var list = new List<TypeConfiguration>
				{
					modelCTypeConfiguration
				};
				var builder = new TypeConfigurationBuilder(list);

				var result = builder.Build(modelCTypeConfiguration, typeof(TestModelC));

				var baseTypeConfigurations = result.BaseTypeConfigurations.Should().HaveCount(2).And.Subject;
				baseTypeConfigurations.ElementAt(0).Type.Should().Be(typeof(TestModelA));
				baseTypeConfigurations.ElementAt(0).Properties.Should().HaveCount(1);
				baseTypeConfigurations.ElementAt(1).Type.Should().Be(typeof(TestModelB));
				baseTypeConfigurations.ElementAt(1).Properties.Should().HaveCount(1);
			}

			[Fact]
			public void CollectsAllProperties()
			{
				var modelWithNestedTypeConfiguration = new TypeConfiguration(typeof(TestModelWithNested));
				var modelNestedTypeConfiguration = new TypeConfiguration(typeof(TestModelNested));
				var list = new List<TypeConfiguration>
				{
					modelWithNestedTypeConfiguration,
					modelNestedTypeConfiguration
				};
				var builder = new TypeConfigurationBuilder(list);

				var result = builder.Build(modelWithNestedTypeConfiguration, typeof(TestModelWithNested));
				result.Properties.Should().HaveCount(4);
				result.Properties.Should()
					.ContainSingle(p => p.PropertyInfo.Name == nameof(TestModelWithNested.Id) && p.Type == typeof(int)).And
					.ContainSingle(p => p.PropertyInfo.Name == nameof(TestModelWithNested.Some1) && p.Type == typeof(string)).And
					.ContainSingle(p => p.PropertyInfo.Name == nameof(TestModelWithNested.Some2) && p.Type == typeof(string));
			}

			[Fact]
			public void CorrectlyResolvesPrimitiveLists()
			{
				var modelPrimitiveCollectionTypeConfiguration = new TypeConfiguration(typeof(TestModelWithPrimitiveList));
				var list = new List<TypeConfiguration>
				{
					modelPrimitiveCollectionTypeConfiguration
				};
				var builder = new TypeConfigurationBuilder(list);

				var result = builder.Build(modelPrimitiveCollectionTypeConfiguration, typeof(TestModelWithPrimitiveList));
				result.Properties.Should().HaveCount(1);
				result.Properties.Should()
					.ContainSingle(p =>
						p.PropertyInfo.Name == nameof(TestModelWithPrimitiveList.Strings) &&
						p.TypeInfoWrapper.IsArray &&
						p.TypeConfiguration == null &&
						p.Type == typeof(string));
			}

			[Fact]
			public void CorrectlyResolvesRawComplexTypes()
			{
				var modelNestedTypeConfiguration = new TypeConfiguration(typeof(TestModelNested));
				var list = new List<TypeConfiguration>
				{
					modelNestedTypeConfiguration
				};
				var builder = new TypeConfigurationBuilder(list);

				var result = builder.Build(modelNestedTypeConfiguration, typeof(TestModelNested));

				result.Properties.Should()
					.ContainSingle(p => p.PropertyInfo.Name == nameof(TestModelNested.Id)).And
					.ContainSingle(p =>
						p.PropertyInfo.Name == nameof(TestModelNested.Nested) &&
						p.TypeConfiguration == null);
			}

			[Fact]
			public void CollectsOnlyDeclaredProperties()
			{
				var modelBTypeConfiguration = new TypeConfiguration(typeof(TestModelB));
				var list = new List<TypeConfiguration>()
				{
					modelBTypeConfiguration
				};
				var builder = new TypeConfigurationBuilder(list);

				var result = builder.Build(modelBTypeConfiguration, typeof(TestModelB));

				var prop = result.Properties.Should().HaveCount(1).And.Subject.First();
				prop.PropertyInfo.Name.Should().Be(nameof(TestModelB.Foo));
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

				var nested = result.Properties.Single(p => p.Type == typeof(TestModelNested));
				nested.TypeConfiguration.Should().Be(modelNestedTypeConfiguration);

				var nestedNested = nested.TypeConfiguration.Properties.Single(p => p.Type == typeof(TestModelNestedNested));
				nestedNested.TypeConfiguration.Should().Be(modelNestedNestedTypeConfiguration);
			}
		}

		public class Step2Test : TestHost
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

				var nested = result.Properties.Single(p => p.Type == typeof(TestModelA));
				nested.TypeConfiguration.Should().Be(modelATypeConfiguration);
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

				var nested = result.Properties.Single(p => p.Type == typeof(TestModelA));
				nested.TypeInfoWrapper.IsArray.Should().Be(true);
			}
		}
	}
}
