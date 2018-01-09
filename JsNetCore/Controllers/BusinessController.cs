using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jint;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JsNetCore.Controllers
{
	[Produces("application/json")]
	[Route("api/Business")]
	public class BusinessController : Controller
	{
		private readonly Engine _engine = new Engine();

		// GET: api/business/method1?a=1&b=2
		[HttpGet("{method}", Name = "Get")]
		public string Get(string method)
		{
			var script = System.IO.File.ReadAllText($"Scripts/{method}.js");
			
			//_engine.SetValue("log", new Action<object>(Console.WriteLine));
			
			/*_engine.Execute(@"
				function hello() {
					log('Hello World');
				};
				hello();");*/
			
			foreach (var param in HttpContext.Request.Query)
			{
				Console.WriteLine(param);
			}

			return method.ToString() + script;
		}
	}
}
