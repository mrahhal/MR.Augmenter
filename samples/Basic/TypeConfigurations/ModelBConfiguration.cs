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
}
