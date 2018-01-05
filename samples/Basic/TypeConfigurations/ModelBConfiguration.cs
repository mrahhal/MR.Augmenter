using Basic.Models;
using MR.Augmenter;

namespace Basic.TypeConfigurations
{
	public class ModelBConfiguration : TypeConfiguration<ModelB>
	{
		public ModelBConfiguration()
		{
			Add("Details2", (x, state) => $"{x.Details}2");
		}
	}

	public class AnotherModelBConfiguration : TypeConfiguration<ModelB>
	{
		public AnotherModelBConfiguration()
		{
			Add("Details3", (x, state) => $"{x.Details}3");
		}
	}

	public abstract class ModelBConfigurationBase : TypeConfiguration<ModelB>
	{
		public ModelBConfigurationBase()
		{
		}
	}

	public class AnotherAnotherModelBConfiguration : ModelBConfigurationBase
	{
		public AnotherAnotherModelBConfiguration()
		{
			Add("Details4", (x, state) => $"{x.Details}4");
		}
	}
}
