using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace MR.Augmenter
{
	public static class AugmenterBuilderExtensions
	{
		/// <summary>
		/// Adds an Mvc filter globally to handle augmenting objects on responses.
		/// </summary>
		/// <param name="builder"></param>
		public static void ForMvc(
			this IAugmenterBuilder builder)
		{
			builder.Services.Configure<MvcOptions>(options =>
			{
				options.Filters.Add(new AugmenterActionFilterAttribute());
			});
		}
	}
}
