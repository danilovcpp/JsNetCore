using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsNetCore.Models
{
    public class RunRequest
    {
        public string TableName { get; set; }
        public string Method { get; set; }
        public ResultTypeEnum ResultType { get; set; }
        public object Params { get; set; }
    }
}
