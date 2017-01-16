using System.Collections.Generic;
using MR.Augmenter;

namespace Basic.Models
{
	public class ModelC : ModelB
	{
		public List<ModelSome> Models { get; set; } = new List<ModelSome>() { new ModelSome(), new ModelSome() };

		public Model1 Normal { get; set; } = new Model1();

		public AugmenterWrapper<Model1> Wrapper { get; set; } = new AugmenterWrapper<Model1>(new Model1());

		public List<AugmenterWrapper<ModelSome>> Wrappers { get; set; } = new List<AugmenterWrapper<ModelSome>>()
		{
			new AugmenterWrapper<ModelSome>(new ModelSome()), new AugmenterWrapper<ModelSome>(new ModelSome())
		};
	}
}
