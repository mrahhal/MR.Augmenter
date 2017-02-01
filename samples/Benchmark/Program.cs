using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using MR.Augmenter;
using Newtonsoft.Json;

namespace Benchmark
{
	public class Bench
	{
		private IServiceProvider _provider;
		private IAugmenter _augmenter;

		[Setup]
		public void SetupData()
		{
			_provider = Helper.CreateProvider();
			_augmenter = _provider.GetService<IAugmenter>();
		}

		[Benchmark]
		public object Serialize()
		{
			var model = Helper.CreateModel();
			return JsonConvert.SerializeObject(model);
		}

		[Benchmark]
		public object Augment()
		{
			var model = Helper.CreateModel();
			var augmented = _augmenter.AugmentAsync(model).Result;
			return JsonConvert.SerializeObject(augmented);
		}
	}

	public class Program
	{
		private const int Iterations = 10000;

		public static void Main(string[] args)
		{
			//new Program().Run();

			var summary = BenchmarkRunner.Run<Bench>();
		}

		private void Run()
		{
			RunAsync().GetAwaiter().GetResult();
		}

		private async Task RunAsync()
		{
			var provider = Helper.CreateProvider();

			// Warm up.
			using (var scope = Helper.CreateScope(provider))
			{
				var p = scope.ServiceProvider;
				await Helper.RunAugmentAsync(p);
				Helper.RunSerialize();
			}

			// Real measuring.
			using (var scope = Helper.CreateScope(provider))
			{
				var p = scope.ServiceProvider;
				var sw = Stopwatch.StartNew();

				for (int i = 0; i < Iterations; i++)
				{
					await Helper.RunAugmentAsync(p);
				}

				var s1 = sw.Elapsed;
				sw.Restart();

				for (int i = 0; i < Iterations; i++)
				{
					Helper.RunSerialize();
				}

				var s2 = sw.Elapsed;
				sw.Stop();

				Print(s1, s2);
			}
		}

		private void Print(TimeSpan s1, TimeSpan s2)
		{
			Console.WriteLine($"S1: {s1.TotalSeconds} secs ({(double)s1.Ticks / s2.Ticks}x)");
			Console.WriteLine($"S2: {s2.TotalSeconds} secs");
		}
	}

	public static class Helper
	{
		public static Task<string> RunAugmentAsync(IServiceProvider p)
		{
			return RunAugmentAsync(GetAugmenter(p));
		}

		public static async Task<string> RunAugmentAsync(IAugmenter augmenter)
		{
			var model = CreateModel();
			var obj = await augmenter.AugmentAsync(model);
			return Serialize(obj);
		}

		public static string RunSerialize()
		{
			var model = CreateModel();
			return Serialize(model);
		}

		public static ModelWithNested CreateModel()
		{
			return new ModelWithNested();
		}

		public static string Serialize(object obj)
		{
			return JsonConvert.SerializeObject(obj);
		}

		public static IAugmenter GetAugmenter(IServiceProvider provider) => provider.GetService<IAugmenter>();

		public static IServiceProvider CreateProvider()
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
					c.Remove(nameof(ModelA.Secret));
				});

				config.Configure<ModelWithNested>(c =>
				{
					c.Add("Something", (x, state) => state["Something"]);
				});

				config.Configure<ModelNested>(c =>
				{
					c.Add("Answer", (x, state) => 42);
				});
			});

			return services.BuildServiceProvider();
		}

		public static IServiceScope CreateScope(IServiceProvider provider)
		{
			return provider.GetService<IServiceScopeFactory>().CreateScope();
		}
	}
}
