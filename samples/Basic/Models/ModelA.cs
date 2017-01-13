using System;

namespace Basic.Models
{
	public class ModelA
	{
		public int Id { get; set; } = 42;

		public string Hash { get; set; } = Guid.NewGuid().ToString("N");

		public string Name { get; set; } = "Someone";

		public string Ex { get; set; } = "Ex";

		public string Secret { get; set; } = Guid.NewGuid().ToString("N");
	}
}
