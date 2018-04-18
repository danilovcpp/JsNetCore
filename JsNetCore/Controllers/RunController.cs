using System;
using System.Collections.Generic;
using System.IO;
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
    [Route("api/[controller]/[action]")]
    public class RunController : Controller
    {
        private readonly Engine _engine = new Engine();
        private readonly Context _context = new Context();
        
        public RunController()
        {
            string[] dirs = Directory.GetFiles(@"Scripts", "*.js");
            string script = "";

            foreach (string file in dirs)
            {
                script += System.IO.File.ReadAllText(file);
            }

            _engine.SetValue("FindByRecid", new Func<string, string, string>(_context.FindByRecid));
            _engine.SetValue("Insert", new Func<string, object, bool>(_context.Insert));
            _engine.SetValue("ExecSqlProcedure", new Func<string, object, object>(_context.ExecSqlProcedure));
            _engine.SetValue("LoadFile", new Func<string, FileStream>(_context.LoadFile)); 
            _engine.SetValue("UpdateByRecid", new Func<string, string, object, bool>(_context.UpdateByRecid));
            _engine.SetValue("FindByParams", new Func<string, object, string>(_context.FindByParams));

            _engine.Execute(script);
        }

        /// <summary>
        /// Метод запускающий на выполнение js функцию
        /// </summary>
        /// <param name="request">Список параметров передаваемых в теле запроса</param>
        /// <returns>Результат выполнения метода в строковом виде</returns>
        [HttpPost(Name = "ExecuteJS")]
        public IActionResult ExecuteJS([FromBody] RunRequest request)
        {
            if(request.ResultType == ResultTypeEnum.FileStream)
            {
                _engine.Execute($"var result = tracks.listen({request.Params})");
                object result = _engine.GetValue("result").ToObject();
                if (result == null)
                    return StatusCode(500);

                return new FileStreamResult((FileStream)result, "audio/mpeg");
            }

            _engine.Execute($"var result = {request.TableName.ToLower()}.{request.Method.ToLower()}({request.Params});");
            return Ok(_engine.GetValue("result").ToString());
        }
        
        // Тестовый метод для воспроизведения трэка
        [HttpGet(Name = "ListenTrack")]
        public IActionResult ListenTrack()
        {
            _engine.Execute($"var result = listen({"{ recId :\"634c67d3-5a80-40c7-8555-9da264a5353f\" }"});");

            object result = _engine.GetValue("result").ToObject();
            if (result == null) return StatusCode(500);

            return new FileStreamResult((FileStream)result, "audio/mpeg");
        }

        // Тестовый метод для получения значения определенного поля
        [HttpGet(Name = "InfoTrack")]
        public IActionResult InfoTrack()
        {
            _engine.Execute($"var newResult = tracks.fields.recid.value;");
            return Ok(_engine.GetValue("newResult").ToString());
        }
    }
}
