using System.Threading.Tasks;
using Basic.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MR.Augmenter;

namespace Basic
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			services.AddAugmenter(config =>
			{
				// Use this if you want to configure some state on every invocation of `AugmentAsync`.
				config.ConfigureGlobalState = (state, provider) =>
				{
					// You can use provider to resolve some services (such as IAuthenticationManager).
					state["IsFoo"] = true; // This state will always be available.

					state["IsAdmin"] = false; // You can resolve this from IAuthenticationManager.

					// Let's try doing something a bit more complex.
					var context = provider.GetService<IHttpContextAccessor>().HttpContext;
					if (context.Request.Path.Value.EndsWith("b"))
					{
						state["Bar"] = "bar";
					}

					return Task.CompletedTask;
				};

				config.Configure<ModelA>(c =>
				{
					c.ConfigureRemove(nameof(ModelA.Ex));
					c.ConfigureRemove(nameof(ModelA.Secret), (x, state) =>
					{
						if ((bool)state["IsAdmin"])
						{
							// Don't remove if IsAdmin is true. Ignore here refers to this specific augmentation.
							return AugmentationValue.Ignore;
						}
						return null;
					});

					c.ConfigureAdd("Some", (x, state) => $"/{x.Hash}/some");
					c.ConfigureAdd("IsFoo", (x, state) => state["IsFoo"]);
					c.ConfigureAdd("Bar", (x, state) =>
					{
						object value;
						if (!state.TryGetValue("Bar", out value))
						{
							// Return this special value to let the service know that it should
							// ignore this augmentation.
							return AugmentationValue.Ignore;
						}
						return value;
					});
				});

				config.Configure<ModelB>(c =>
				{
					c.ConfigureAdd("Details2", (x, state) => $"{x.Details}2");
				});
			});

			services.AddMvc(options =>
			{
				options.Filters.Add(new AugmenterActionFilterAttribute());
			})
			.AddJsonOptions(options =>
			{
				options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
			});
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMvc();
		}
	}
}
