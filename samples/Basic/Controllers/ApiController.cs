using Basic.Models;
using Microsoft.AspNetCore.Mvc;
using MR;
using MR.Augmenter;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
			var wrapper = new AugmenterWrapper<ModelB>(model);
			wrapper.SetConfiguration(c =>
			{
				c.Add("Baz", (x, state) => x.Id);
			});

			return Ok(wrapper);
		}

		[HttpGet("wrapper-nested")]
		public IActionResult GetWrapperNested()
		{
			// You can use the special AugmenterWrapper to wrap your model with
			// some configuration that will be used when doing the augmentation.
			var model = new ModelB();
			var wrapper = new AugmenterWrapper<ModelB>(model);
			wrapper.SetConfiguration(c =>
			{
				c.Add("Baz", (x, state) => x.Id);
			});

			return Ok(new
			{
				Model = wrapper
			});
		}

		[HttpGet("complex")]
		public IActionResult GetComplex()
		{
			var model = new ModelC();

			return Ok(model);
		}

		[HttpGet("camel")]
		public IActionResult GetCamel()
		{
			// Json options will be honored.
			var model = new ModelB();

			return Json(model, new JsonSerializerSettings()
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				Formatting = Formatting.Indented
			});
		}

		[HttpGet("camel-complex")]
		public IActionResult GetCamelComplex()
		{
			var model = new ModelC();

			return Json(model, new JsonSerializerSettings()
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				Formatting = Formatting.Indented
			});
		}

		[HttpGet("n1")]
		public IActionResult GetN1()
		{
			var model = new ModelU2();

			return Ok(model);
		}

		[HttpGet("g1")]
		public IActionResult GetG1()
		{
			var model = new GracefulExpandoObject();
			model["Foo"] = "foo";

			return Ok(new[] { model });
		}
	}
}
