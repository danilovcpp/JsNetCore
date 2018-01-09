using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jint;
using JsNetCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JsNetCore.Controllers
{
	[Produces("application/json")]
	[Route("api/Business")]
	public class BusinessController : Controller
	{
		private readonly Engine _engine = new Engine();
		private readonly Context _context = new Context();
		
		// GET: api/business/method1?a=1&b=2
		[HttpGet("{method}", Name = "Get")]
		public string Get(string method)
		{
			var script = System.IO.File.ReadAllText($"Scripts/{method}.js");
			
			_engine.SetValue("exec", new Func<string, string>(_context.Exec));
			
			_engine.Execute(@"var result = exec('Hello World');");
			var result = _engine.GetValue("result");
			
			foreach (var param in HttpContext.Request.Query)
			{
				Console.WriteLine(param);
			}

			return result.ToString();
		}
	}
}
