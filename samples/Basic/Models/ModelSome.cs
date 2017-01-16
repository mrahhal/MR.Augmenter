using System;

namespace Basic.Models
{
	public class ModelSome
	{
		public int Id { get; set; } = 42;

		public string Hash { get; set; } = Guid.NewGuid().ToString("N");
	}
}
