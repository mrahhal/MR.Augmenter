using Basic.Models;
using MR.Augmenter;

namespace Basic.TypeConfigurations
{
	public class InterfaceAConfiguration : TypeConfiguration<IInterfaceA>
	{
		public InterfaceAConfiguration()
		{
			Add("Some", (_, __) => "Some!");
			Custom((x, state) =>
			{
				x.Id++;
			});
		}
	}
}
