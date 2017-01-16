namespace Basic.Models
{
	public class ModelU1
	{
		public int Id { get; set; } = 42;
		public string Some { get; set; } = "43";
		public ModelU3 Model { get; set; } = new ModelU3();
	}

	public class ModelU2 : ModelU1
	{
		public string Name { get; set; } = "foo";
	}

	public class ModelU3
	{
		public int Id { get; set; } = 44;
	}
}
