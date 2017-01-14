using Basic.Models;
using Microsoft.AspNetCore.Mvc;
using MR.Augmenter;

namespace Basic.Controllers
{
	[Route("[controller]")]
	public class ApiController : Controller
	{
		private IAugmenter _augmenter;

		public ApiController(IAugmenter augmenter)
		{
			_augmenter = augmenter;
		}

		[HttpGet("a")]
		public IActionResult GetA()
		{
			var model = new ModelA();

			return Ok(model);
		}

		[HttpGet("b")]
		public IActionResult GetB()
		{
			// Inheritance works! ModelA's augments will be applied to ModelB.
			var model = new ModelB();

			return Ok(model);
		}

		[HttpGet("anon")]
		public IActionResult GetAnon()
		{
			var model = new ModelB();

			// Works with anonymous objects!
			return Ok(new
			{
				Foo = "foo",
				Inner = model
			});
		}

		[HttpGet("list")]
		public IActionResult GetList()
		{
			var list = new[]
			{
				new ModelB() { Id = 1 },
				new ModelB() { Id = 2 }
			};

			// Works with lists too.
			return Ok(list);
		}

		[HttpGet("list2")]
		public IActionResult GetList2()
		{
			var list = new[]
			{
				new ModelB() { Id = 1 },
				new ModelB() { Id = 2 }
			};

			// Works with lists too.
			return Ok(new
			{
				Models = list
			});
		}

		[HttpGet("wrapper")]
		public IActionResult GetWrapper()
		{
			// You can use the special AugmenterWrapper to wrap your model with
			// some configuration that will be used when doing the augmentation.
			var model = new ModelB();
			var wrapper = new AugmenterWrapper(model);
			wrapper.SetConfiguration<ModelB>(c =>
			{
				c.ConfigureAdd("Baz", (x, state) => x.Id);
			});

			return Ok(wrapper);
		}
	}
}
