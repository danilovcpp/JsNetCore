using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jint;
using JsNetCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
		public object Get(string method)
		{
			var script = System.IO.File.ReadAllText($"Scripts/{method}.js");
			
			_engine.SetValue("exec", new Func<string, string>(_context.Exec));

			var parameters = String.Empty;
			foreach (var param in HttpContext.Request.Query)
			{
				parameters += $"{param.Value},";
			}
			
			if(parameters.EndsWith(","))
			{
				parameters = parameters.Remove(parameters.Length - 1);
			}

			_engine.Execute($"var result = {script}({parameters});");
			var result = _engine.GetValue("result");

			return result.ToObject();
		}
	}
}
