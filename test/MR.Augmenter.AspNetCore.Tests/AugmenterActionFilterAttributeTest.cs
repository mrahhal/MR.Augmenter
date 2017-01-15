using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MR.Augmenter
{
	public class AugmenterActionFilterAttributeTest
	{
		[Fact]
		public async Task WithViewResult_Ignores()
		{
			var mockAugmenter = MockAugmenter();
			var result = new ViewResult();

			await Execute(mockAugmenter, result);

			mockAugmenter.Verify(x => x.AugmentCore(), Times.Never);
		}

		[Fact]
		public async Task WithObjectResult()
		{
			var mockAugmenter = MockAugmenter();
			var result = new OkObjectResult(new TestModel());

			await Execute(mockAugmenter, result);

			mockAugmenter.Verify(x => x.AugmentCore(), Times.Once);
		}

		[Fact]
		public async Task WithJsonResult()
		{
			var mockAugmenter = MockAugmenter();
			var result = new JsonResult(new TestModel());

			await Execute(mockAugmenter, result);

			mockAugmenter.Verify(x => x.AugmentCore(), Times.Once);
		}

		private Task Execute(Mock<AugmenterStub> mockAugmenter, IActionResult result)
		{
			var typeFilter = new AugmenterActionFilterAttribute();
			var services = new ServiceCollection();
			services.AddSingleton<IAugmenter>(mockAugmenter.Object);
			var provider = services.BuildServiceProvider();
			var filter = typeFilter.CreateInstance(provider) as IAsyncResultFilter;

			var context = CreateResultExecutingContext(filter, result);
			var next = new ResultExecutionDelegate(() => Task.FromResult(CreateResultExecutedContext(context, result)));

			return filter.OnResultExecutionAsync(context, next);
		}

		private Mock<AugmenterStub> MockAugmenter()
		{
			var mock = new Mock<AugmenterStub>() { CallBase = true };
			return mock;
		}

		private static ResultExecutingContext CreateResultExecutingContext(
			IFilterMetadata filter,
			IActionResult result)
		{
			return new ResultExecutingContext(
				CreateActionContext(),
				new IFilterMetadata[] { filter, },
				result,
				controller: new object());
		}

		private static ResultExecutedContext CreateResultExecutedContext(
			ResultExecutingContext context,
			IActionResult result)
		{
			return new ResultExecutedContext(context, context.Filters, result, context.Controller);
		}

		private static ActionContext CreateActionContext()
		{
			return new ActionContext(Mock.Of<HttpContext>(), new RouteData(), new ActionDescriptor());
		}

		private class TestModel
		{
		}

		public class AugmenterStub : IAugmenter
		{
			public Task<object> AugmentAsync<T>(
				T obj,
				Action<TypeConfiguration<T>> configure = null,
				Action<Dictionary<string, object>> addState = null)
			{
				return AugmentCore();
			}

			public Task<object> AugmentAsync<T>(IEnumerable<T> list, Action<TypeConfiguration<T>> configure = null, Action<Dictionary<string, object>> addState = null)
			{
				return AugmentCore();
			}

			public Task<object> AugmentAsync<T>(T[] list, Action<TypeConfiguration<T>> configure = null, Action<Dictionary<string, object>> addState = null)
			{
				return AugmentCore();
			}

			public virtual Task<object> AugmentCore()
			{
				return Task.FromResult((object)null);
			}
		}
	}
}
