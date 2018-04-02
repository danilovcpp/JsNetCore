﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jint;
using JsNetCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JsNetCore.Controllers
{
    [Produces("application/json")]
    [Route("api/Run")]
    public class RunController : Controller
    {
        private readonly Engine _engine = new Engine();
        private readonly Context _context = new Context();

        /// <summary>
        /// Метод запускающий на выполнение js функцию
        /// </summary>
        /// <param name="request">Список параметров передаваемых в теле запроса</param>
        /// <returns>Результат выполнения метода в строковом виде</returns>
        [HttpPost(Name = "Run")]
        public IActionResult Run([FromBody] RunRequest request)
        {
            string fileName = $"Scripts/{request.TableName}.js";

            if (!System.IO.File.Exists(fileName))
                return NotFound();

            var script = System.IO.File.ReadAllText(fileName);

            _engine.SetValue("FindById", new Func<string, string, string>(_context.FindById));
            _engine.SetValue("Save", new Func<string, object, bool>(_context.Save));

            _engine.Execute(script);
            _engine.Execute($"var result = {request.Method}({request.Params});");

            return Ok(_engine.GetValue("result").ToString());
        }
    }
}