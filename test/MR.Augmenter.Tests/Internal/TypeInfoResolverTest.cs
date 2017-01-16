using System;
using FluentAssertions;
using Xunit;

namespace MR.Augmenter.Internal
{
	public class TypeInfoResolverTest
	{
		[Fact]
		public void Primitive()
		{
			var result = TypeInfoResolver.ResolveTypeInfo(typeof(int));

			result.IsPrimitive.Should().BeTrue();
		}

		[Fact]
		public void Normal()
		{
			var result = TypeInfoResolver.ResolveTypeInfo(typeof(TestModel1));

			result.Should().NotBeNull();
			result.Type.Should().Be(typeof(TestModel1));
			result.IsArray.Should().Be(false);
			result.IsWrapper.Should().Be(false);
		}

		[Fact]
		public void Array()
		{
			var model = new[] { new TestModel1(), new TestModel1() };

			var result = TypeInfoResolver.ResolveTypeInfo(model.GetType());

			result.Should().NotBeNull();
			result.Type.Should().Be(typeof(TestModel1));
			result.IsArray.Should().Be(true);
			result.IsWrapper.Should().Be(false);
		}

		[Fact]
		public void Wrapper()
		{
			var model = new AugmenterWrapper<TestModel1>(new TestModel1());

			var result = TypeInfoResolver.ResolveTypeInfo(model.GetType());

			result.Should().NotBeNull();
			result.Type.Should().Be(typeof(TestModel1));
			result.IsArray.Should().Be(false);
			result.IsWrapper.Should().Be(true);
		}

		[Fact]
		public void ArrayAndWrapper()
		{
			var model = new[] { new AugmenterWrapper<TestModel1>(new TestModel1()), new AugmenterWrapper<TestModel1>(new TestModel1()) };

			var result = TypeInfoResolver.ResolveTypeInfo(model.GetType());

			result.Should().NotBeNull();
			result.Type.Should().Be(typeof(TestModel1));
			result.IsArray.Should().Be(true);
			result.IsWrapper.Should().Be(true);
		}
	}
}
