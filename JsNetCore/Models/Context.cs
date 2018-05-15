using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Net.Http;
using System.IO.Compression;

namespace JsNetCore.Models
{
	public class Context
	{
		private string connectionString = "Server=localhost;Port=5432;User Id = postgres; Password=postgres;Database=ownRadioRdev;";

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
									if (!reader.IsDBNull(i))
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

		#region Поиск в таблице по UUID
		/// <summary>
		/// Поиск строки в таблице по id
		/// </summary>
		/// <param name="tableName">Название таблицы</param>
		/// <param name="id"> UUID по которому получаем значения строки в таблице</param>
		/// <returns>Результат выборки из таблицы в строковом виде в формате json</returns>
		public string FindByRecid(string tableName, string id)
		{
			var obj = new JObject();

			using (var npgSqlConnection = new NpgsqlConnection(connectionString))
			{
				try
				{
					using (var npgSqlCommand = new NpgsqlCommand($"SELECT * FROM {tableName} WHERE recid = '{id}'"))
					{
						npgSqlConnection.Open();
						npgSqlCommand.Connection = npgSqlConnection;

						NpgsqlDataReader reader = npgSqlCommand.ExecuteReader();

						if (reader.Read())
						{
							for (int i = 0; i < reader.FieldCount; i++)
							{
								obj.Add(reader.GetName(i), reader.GetValue(i).ToString());
							}
						}
					}
				}
				catch (Exception)
				{
					return string.Empty;
				}
			}

			return obj.Count > 0 ? JsonConvert.SerializeObject(obj) : string.Empty;
		}
		#endregion

		#region Добавление строки в таблицу
		/// <summary>
		/// Метод добавления строки в таблицу
		/// </summary>
		/// <param name="tableName">Название таблицы</param>
		/// <param name="parameters">Набор полей таблицы</param>
		/// <returns>Результат выполнения операции</returns>
		public MethodResult Insert(string tableName, object parameters)
		{
			var result = new MethodResult();
			string[] param = ParseQueryParameters(parameters);

			using (var npgSqlConnection = new NpgsqlConnection(connectionString))
			{
				try
				{
					using (var npgSqlCommand = new NpgsqlCommand($"INSERT INTO {tableName}({param[0]}) VALUES({param[1]})"))
					{
						npgSqlConnection.Open();
						npgSqlCommand.Connection = npgSqlConnection;
						npgSqlCommand.ExecuteNonQuery();

						result.Success = true;
					}
				}
				catch (Exception ex)
				{
					result.Message = ex.Message;
					result.Success = false;
				}
			}

			return result;
		}
		#endregion

		#region Выполнение SQL процедур

		public object ExecSqlProcedure(string functionName, object parameters)
		{
			var par = JObject.FromObject(parameters);
			object nextTrackID;

			using (var npgSqlConnection = new NpgsqlConnection(connectionString))
			{
				var npgSqlCommand = new NpgsqlCommand();
				npgSqlCommand.CommandText = functionName;
				npgSqlCommand.Connection = npgSqlConnection;
				npgSqlCommand.CommandType = CommandType.StoredProcedure;

				foreach (var x in par)
				{
					npgSqlCommand.Parameters.AddWithValue(x.Key.ToLower(), Guid.Parse(x.Value.ToString()));
				}

				npgSqlConnection.Open();
				nextTrackID = npgSqlCommand.ExecuteScalar();
			}
			return nextTrackID;
		}
		#endregion

		#region Получение содержимого файла
		/// <summary>
		/// Получение содержимого файла
		/// </summary>
		/// <param name="path">Путь к файлу </param>
		/// <returns>Байт массив(при отладке текстовое содержимое файла)</returns>
		public FileStream LoadFile(string path)
		{
			if (!File.Exists(path))
				return null;

			return File.OpenRead(path);
		}
		#endregion

		#region Обновление данных таблицы
		/// <summary>
		/// Метод обновления значения в таблице
		/// </summary>
		/// <param name="tableName">Название таблицы</param>
		/// <param name="parameters">Параметры</param>
		/// <returns>Результат выполнения операции, true/false</returns>
		public bool UpdateByRecid(string tableName, string recid, object parameters)
		{
			string[] param = ParseQueryParameters(parameters);

			using (var npgSqlConnection = new NpgsqlConnection(connectionString))
			{
				try
				{
					using (var npgSqlCommand = new NpgsqlCommand($"UPDATE {tableName} SET ({param[0]}) = ({param[1]}) WHERE recid = '{recid}'"))
					{
						npgSqlConnection.Open();
						npgSqlCommand.Connection = npgSqlConnection;
						npgSqlCommand.ExecuteNonQuery();
					}
				}
				catch (Exception)
				{
					return false;
				}
			}

			return true;
		}
		#endregion

		#region Поиск в таблице по параметрам
		/// <summary>
		/// Поиск в таблице по параметрам
		/// </summary>
		/// <param name="tableName">Название таблицы</param>
		/// <param name="parameters">Параметры запроса</param>
		/// <returns>Результат SQL запроса</returns>
		public string FindByParams(string tableName, object parameters)
		{
			var objArray = new JArray();
			string[] param = ParseQueryParameters(parameters);

			using (var npgSqlConnection = new NpgsqlConnection(connectionString))
			{
				try
				{
					using (var npgSqlCommand = new NpgsqlCommand($"SELECT * FROM {tableName} WHERE ({param[0]}) = ({param[1]})"))
					{
						npgSqlConnection.Open();
						npgSqlCommand.Connection = npgSqlConnection;

						NpgsqlDataReader reader = npgSqlCommand.ExecuteReader();

						while (reader.Read())
						{
							var obj = new JObject();

							for (int i = 0; i < reader.FieldCount; i++)
							{
								obj.Add(reader.GetName(i), reader.GetValue(i).ToString());
							}

							objArray.Add(obj);
						}
					}
				}
				catch (Exception)
				{
					return string.Empty;
				}
			}

			return objArray.Count > 0 ? JsonConvert.SerializeObject(objArray) : string.Empty;
		}
		#endregion

		public async Task<MethodResult> SaveFile(string base64File, string type)
		{
			var result = new MethodResult();

			try
			{
				string name = Guid.NewGuid().ToString() + type;

				string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Files");
				string filePath = Path.Combine(directoryPath, name);

				if (!Directory.Exists(directoryPath))
					Directory.CreateDirectory(directoryPath);

				byte[] file = Convert.FromBase64String(base64File);
				await File.WriteAllBytesAsync(filePath, file);

				result.Data = name;
				result.Success = true;
			}
			catch(Exception ex)
			{
				result.Message = ex.Message;
				result.Success = false;
			}

			return result;
		}

		public async Task<MethodResult> DownloadFile(string url)
		{
			var result = new MethodResult() { Success = false };
			HttpResponseMessage response = null;
			Stream stream = null;

			try
			{
				using (HttpClientHandler handler = new HttpClientHandler())
				{
					handler.AllowAutoRedirect = false;
					using (HttpClient httpClient = new HttpClient(handler))
					{
						httpClient.Timeout = TimeSpan.FromMinutes(30);

						using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url))
						using (response = await httpClient.SendAsync(request))
						{
							var location = response.Headers.Location;
							if (location != null)
							{
								response = await httpClient.GetAsync(location.OriginalString);
								stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
							}
							else
							{
								stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
							}

							string path = Path.Combine(
								Directory.GetCurrentDirectory(), 
								"Download", Guid.NewGuid().ToString() + "." + response.Content.Headers.ContentType.MediaType.Split("/").ToString()
							);

							using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
							{ 
								await stream.CopyToAsync(fs);
							}

							result.Data = path;
							result.Success = true;
						}
					}
				}
			}
			catch(Exception ex)
			{
				result.Message = ex.Message;
				result.Success = false;
			}

			return result;
		}

		public MethodResult UnpackArchive(string srcPath)
		{
			var result = new MethodResult() { Success = false };
			var model = new List<FileModel>();

			var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Files");

			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			try
			{
				using (var zip = ZipFile.OpenRead(srcPath))
				{
					foreach (var entry in zip.Entries)
					{
						Guid localFileName = Guid.NewGuid();
						string filePath = $"{directoryPath}\\{localFileName.ToString()}" + Path.GetExtension(entry.FullName);

						model.Add(new FileModel
						{
							LocalFileName = localFileName,
							FileName = entry.Name,
							Path = filePath
						});

						entry.ExtractToFile(filePath);
					}

					result.Data = model;
				}
			}
			catch(Exception ex)
			{
				result.Message = ex.Message;
				result.Success = false;
			}

			return result;
		}

		private string[] ParseQueryParameters(object parameters)
		{
			if (parameters == null)
				return null;

			var par = JObject.FromObject(parameters);
			Dictionary<string, string> param = new Dictionary<string, string>();

			foreach (var x in par)
			{
				param.Add(x.Key.ToLower(), $"'{x.Value.ToString()}'");
			}

			string cmdParams = String.Join(',', param.Keys);
			string cmdArgs = String.Join(',', param.Values);

			return new string[] { cmdParams, cmdArgs };
		}
	}
}
