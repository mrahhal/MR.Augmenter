using System;

namespace Basic.Models
{
	public class ModelA
	{
		public int Id { get; set; }

		public string Hash { get; set; }

		public string Name { get; set; }

		public string Ex { get; set; }

		public string Secret { get; set; } = Guid.NewGuid().ToString("N");
	}
}
