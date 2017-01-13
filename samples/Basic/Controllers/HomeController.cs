using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
	[Route("")]
	public class HomeController : Controller
	{
		[HttpGet]
		public IActionResult Get()
		{
			return Ok("Use one of the api endpoints...");
		}
	}
}
