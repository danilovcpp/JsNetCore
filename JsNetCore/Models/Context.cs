using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace JsNetCore.Models
{
	public class Context
	{
		public string Exec(string query)
		{
			var connString = "Host=localhost;Username=;Password=;Database=";

			var list = new JArray();

			using (var conn = new NpgsqlConnection(connString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand(query, conn))
				using (var reader = cmd.ExecuteReader())
					while (reader.Read())
					{
						var @object = new JObject();

						for (int i = 0; i < reader.FieldCount; i++)
						{
							var columnName = reader.GetName(i);
							var type = reader.GetDataTypeName(i);
							
							switch (type.ToString())
							{
								case "uuid":
									@object.Add(columnName, reader.GetValue(i).ToString());
									break;
								case "text":
									@object.Add(columnName, reader.GetValue(i).ToString());
									break;
								case "boolean":
									@object.Add(columnName, reader.GetFieldValue<bool>(i));
									break;
								case "timestamp":
									@object.Add(columnName, reader.GetValue(i).ToString());
									break;
								case "date":
									@object.Add(columnName, reader.GetValue(i).ToString());
									break;
								case "time":
									@object.Add(columnName, reader.GetValue(i).ToString());
									break;
								case "bigint":
									@object.Add(columnName, reader.GetFieldValue<long>(i));
									break;
								case "int4":
									if(!reader.IsDBNull(i))
										@object.Add(columnName, reader.GetInt32(i));
									else
										@object.Add(columnName, null);
									break;
								case "decimal":
									@object.Add(columnName, reader.GetFieldValue<int>(i));
									break;
								case "float8":
									if (!reader.IsDBNull(i))
										@object.Add(columnName, reader.GetFloat(i));
									else
										@object.Add(columnName, null);
									break;
								default:
									@object.Add(columnName, reader.GetValue(i).ToString());
									break;
							}
						}

						list.Add(@object);
					}
			}

			return JsonConvert.SerializeObject(list);
		}
	}
}
