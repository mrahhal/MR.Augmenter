using System.Collections.Generic;

namespace Basic.Models
{
	public interface IModelD
	{
		List<string> Names { get; set; }
	}

	public class ModelD : IModelD
	{
		public int Id { get; set; }

		public List<string> Names { get; set; } = new List<string>();
	}
}
