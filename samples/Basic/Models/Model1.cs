using System.Collections.Generic;

namespace Basic.Models
{
	public class Model1
	{
		public List<int> Integers { get; set; } = new List<int>() { 1, 2, 3, 4, 5 };

		public List<string> Strings { get; set; } = new List<string>() { "foo", "bar", "baz" };

		public List<Model2> Models { get; set; } = new List<Model2>() { new Model2(), new Model2() };
	}
}
