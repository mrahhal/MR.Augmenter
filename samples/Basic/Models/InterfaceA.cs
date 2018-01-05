namespace Basic.Models
{
	public interface IInterfaceA
	{
		int Id { get; set; }
	}

	public class InterfaceAImpl : IInterfaceA
	{
		public int Id { get; set; }
	}
}
