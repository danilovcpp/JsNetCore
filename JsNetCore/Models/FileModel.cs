using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsNetCore.Models
{
	public class FileModel
	{
		public Guid LocalFileName { get; set; }
		public string FileName { get; set; }
		public string Path { get; set; }
	}
}
