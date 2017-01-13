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
	}
}
