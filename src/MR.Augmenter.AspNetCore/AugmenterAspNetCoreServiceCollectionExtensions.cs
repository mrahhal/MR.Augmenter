using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace MR.Augmenter
{
    public static class AugmenterAspNetCoreServiceCollectionExtensions
    {
		public static void AddAugmenterForMvc(
			this IServiceCollection services)
		{
			services.Configure<MvcOptions>(options =>
			{
				options.Filters.Add(new AugmenterActionFilterAttribute());
			});
		}
	}
}
