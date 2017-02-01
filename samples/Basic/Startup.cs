using System.Reflection;
using System.Threading.Tasks;
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
					state["IsFoo"] = Boxed.True; // This state will always be available.

					state["IsAdmin"] = Boxed.False; // You can resolve this from IAuthenticationManager.

					// Let's try doing something a bit more complex.
					var context = provider.GetService<IHttpContextAccessor>().HttpContext;
					if (context.Request.Path.Value.EndsWith("b"))
					{
						state["Bar"] = "bar";
					}

					return Task.CompletedTask;
				};

				// Add this assembly to the ones that should be scanned for TypeConfigurations.
				config.AddAssembly(typeof(Startup).GetTypeInfo().Assembly);

				// These are now each its own class that extends TypeConfiguration
				//config.Configure<ModelA>(c =>
				//{
				//});
				//config.Configure<ModelB>(c =>
				//{
				//});
			}).ForMvc();

			services.AddMvc()
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
