using System;

namespace MR.Augmenter
{
	public abstract class TestHost : IDisposable
	{
		public virtual void Dispose()
		{
		}

		protected AugmenterConfiguration CreateCommonConfiguration(bool build = true)
		{
			var configuration = new AugmenterConfiguration();

			configuration.Configure<TestModel1>(c =>
			{
				c.Add("Bar", (x, _) => $"({x.Id})");
				c.Remove(nameof(TestModel1.Some));
			});
			configuration.Configure<TestModelA>(c =>
			{
				c.Add("Id2", (x, _) => $"{x.Id}2");
			});
			configuration.Configure<TestModelB>(c =>
			{
				c.Remove(nameof(TestModelB.Foo));
			});
			configuration.Configure<TestModelC>(c =>
			{
				c.Add("Bar2", (x, _) => $"{x.Id}-{x.Foo}");
				c.Remove(nameof(TestModel1.Some));
			});

			if (build)
			{
				configuration.Build();
			}

			return configuration;
		}

		protected AugmenterConfiguration CreateBuiltConfiguration()
		{
			var configuration = new AugmenterConfiguration();
			configuration.Configure<TestModel1>(c => { });
			configuration.Build();
			return configuration;
		}
	}
}
