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
    [Route("api/Track")]
    public class TrackController : Controller
    {
        // Создаем объект интерпретатора js для C# кода
        private readonly Engine _engine = new Engine();

        // Создаем объектную модель C#
        private readonly Context _context = new Context();

        /// <summary>
        /// Метод получения трека по id, срабатывает на запрос следующего вида:
        /// GET: api/track/trackId
        /// </summary>
        /// <param name="trackId">Уникальный идентификатор трека</param>
        /// <returns>Трек в виде массива байтов</returns>
        [HttpGet("{trackId}", Name = "GetTrack")]
        public object GetTrack(string trackId)
        {
            var script = System.IO.File.ReadAllText($"Scripts/objectModel.js");
            script += System.IO.File.ReadAllText($"Scripts/tracks.js");

            _engine.Execute(script);

            // Пробрасываем C# методы в js 
            _engine.SetValue("GetById", new Func<string, string>(_context.GetById));
            _engine.SetValue("Insert", new Action<object>(_context.Insert));
            _engine.SetValue("Update", new Action<string, object>(_context.Update));

            //Получаем результат выполнения js кода
            _engine.Execute($"var result = run({trackId})");

            return _engine.GetValue("result").ToObject();
        }
    }
}