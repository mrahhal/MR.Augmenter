using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MR.Augmenter;
using Newtonsoft.Json;

namespace Benchmark
{
	public class Program
	{
		private const int Iterations = 10000;

		public static void Main(string[] args)
		{
			new Program().Run();
		}

		private void Run()
		{
			RunAsync().GetAwaiter().GetResult();
		}

		private async Task RunAsync()
		{
			var provider = CreateProvider();

			// Warm up.
			using (var scope = CreateScope(provider))
			{
				var p = scope.ServiceProvider;
				await RunAugmentAsync(p);
				RunSerialize();
			}

			// Real measuring.
			using (var scope = CreateScope(provider))
			{
				var p = scope.ServiceProvider;
				var sw = Stopwatch.StartNew();

				for (int i = 0; i < Iterations; i++)
				{
					await RunAugmentAsync(p);
				}

				var s1 = sw.Elapsed;
				sw.Restart();

				for (int i = 0; i < Iterations; i++)
				{
					RunSerialize();
				}

				var s2 = sw.Elapsed;
				sw.Stop();

				Print(s1, s2);
			}
		}

		private void Print(TimeSpan s1, TimeSpan s2)
		{
			Console.WriteLine($"S1: {s1.TotalSeconds} secs ({(double)s1.Ticks/s2.Ticks}x)");
			Console.WriteLine($"S2: {s2.TotalSeconds} secs");
		}

		private Task<string> RunAugmentAsync(IServiceProvider p)
		{
			return RunAugmentAsync(GetAugmenter(p));
		}

		private async Task<string> RunAugmentAsync(IAugmenter augmenter)
		{
			var model = CreateModel();
			var obj = await augmenter.AugmentAsync(model);
			return Serialize(obj);
		}

		private string RunSerialize()
		{
			var model = CreateModel();
			return Serialize(model);
		}

		private ModelWithNested CreateModel()
		{
			return new ModelWithNested();
		}

		private string Serialize(object obj)
		{
			return JsonConvert.SerializeObject(obj);
		}

		private IAugmenter GetAugmenter(IServiceProvider provider) => provider.GetService<IAugmenter>();

		private IServiceProvider CreateProvider()
		{
			var services = new ServiceCollection();
			services.AddScoped<ISomeService, SomeService>();
			services.AddAugmenter(config =>
			{
				config.ConfigureGlobalState = async (state, p) =>
				{
					var service = p.GetService<ISomeService>();
					var something = await service.FindSomething();
					state["Something"] = something;
				};

				config.Configure<ModelA>(c =>
				{
					c.ConfigureRemove(nameof(ModelA.Secret));
				});

				config.Configure<ModelWithNested>(c =>
				{
					c.ConfigureAdd("Something", (x, state) => state["Something"]);
				});

				config.Configure<ModelNested>(c =>
				{
					c.ConfigureAdd("Answer", (x, state) => 42);
				});
			});

			return services.BuildServiceProvider();
		}

		private IServiceScope CreateScope(IServiceProvider provider)
		{
			return provider.GetService<IServiceScopeFactory>().CreateScope();
		}
	}
}
