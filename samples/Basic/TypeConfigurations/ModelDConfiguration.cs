using System.Collections;
using Basic.Models;
using MR.Augmenter;

namespace Basic.TypeConfigurations
{
	public class ModelDConfiguration : TypeConfiguration<IModelD>
	{
		public ModelDConfiguration()
		{
			Add("Name", (x, state) =>
			{
				var names = x.GetType().GetProperty("Names").GetValue(x) as IList;
				var name = names.Count > 0 ? names[0] : null;
				return name;
			});

			Remove("Names");
		}
	}
}
