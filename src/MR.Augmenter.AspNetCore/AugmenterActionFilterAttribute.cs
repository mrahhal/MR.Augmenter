using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MR.Augmenter
{
	public class AugmenterActionFilterAttribute : TypeFilterAttribute
	{
		public AugmenterActionFilterAttribute() : base(typeof(AugmenterActionFilterImpl))
		{
		}

		private class AugmenterActionFilterImpl : ActionFilterAttribute
		{
			private IAugmenter _augmenter;

			public AugmenterActionFilterImpl(IAugmenter augmenter)
			{
				_augmenter = augmenter;
			}

			public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
			{
				if (context.Result is ViewResult)
				{
					return next.Invoke();
				}

				var objectResult = context.Result as ObjectResult;
				if (objectResult == null)
				{
					return next.Invoke();
				}

				return OnResultExecutionCoreAsync(context, next, objectResult);
			}

			private async Task OnResultExecutionCoreAsync(
				ResultExecutingContext context,
				ResultExecutionDelegate next,
				ObjectResult result)
			{
				var augmented = await _augmenter.AugmentAsync(result.Value);
				result.Value = augmented;
				await next.Invoke();
			}
		}
	}
}
