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
		/// <returns>MethodResult</returns>
		public MethodResult FindByRecid(string tableName, string id)
		{
			var obj = new JObject();
			var result = new MethodResult();

			try
			{
				using (var npgSqlConnection = new NpgsqlConnection(connectionString))
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

					result.Data = obj.Count > 0 ? JsonConvert.SerializeObject(obj) : null;
					result.Success = true;
				}
			}
			catch (Exception ex)
			{
				result.Message = $"Не удалось получить сведения из таблицы {tableName}:{ex.Message}";
				result.Success = false;
			}

			return result;
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

			try
			{
				string[] param = ParseQueryParameters(parameters);

				using (var npgSqlConnection = new NpgsqlConnection(connectionString))
				using (var npgSqlCommand = new NpgsqlCommand($"INSERT INTO {tableName}({param[0]}) VALUES({param[1]})"))
				{
					npgSqlConnection.Open();
					npgSqlCommand.Connection = npgSqlConnection;
					npgSqlCommand.ExecuteNonQuery();
				}

				result.Success = true;
			}
			catch (Exception ex)
			{
				result.Message = $"Не удалось добавить значение в таблицу {tableName}:{ex.Message}";
				result.Success = false;
			}

			return result;
		}
		#endregion

		#region Выполнение SQL процедур
		public MethodResult ExecSqlProcedure(string functionName, object parameters)
		{
			var result = new MethodResult();
			object nextTrackID = null;

			try
			{
				var par = JObject.FromObject(parameters);
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

					result.Data = nextTrackID;
					result.Success = true;
				}
			}
			catch (Exception ex)
			{
				result.Message = ex.Message;
				result.Success = false;
			}

			return result;
		}
		#endregion

		#region Получение содержимого файла
		/// <summary>
		/// Получение содержимого файла
		/// </summary>
		/// <param name="path">Путь к файлу </param>
		/// <returns>Байт массив(при отладке текстовое содержимое файла)</returns>
		public MethodResult LoadFile(string path)
		{
			var result = new MethodResult();

			try
			{
				result.Data = File.OpenRead(path);
				result.Success = true;
			}
			catch (Exception ex)
			{
				result.Message = ex.Message;
				result.Success = false;
			}

			return result;
		}
		#endregion

		#region Обновление данных таблицы
		/// <summary>
		/// Метод обновления значения в таблице
		/// </summary>
		/// <param name="tableName">Название таблицы</param>
		/// <param name="parameters">Параметры</param>
		/// <returns>Результат выполнения операции, true/false</returns>
		public MethodResult UpdateByRecid(string tableName, string recid, object parameters)
		{
			var result = new MethodResult();

			try
			{
				string[] param = ParseQueryParameters(parameters);

				using (var npgSqlConnection = new NpgsqlConnection(connectionString))
				using (var npgSqlCommand = new NpgsqlCommand($"UPDATE {tableName} SET ({param[0]}) = ({param[1]}) WHERE recid = '{recid}'"))
				{
					npgSqlConnection.Open();
					npgSqlCommand.Connection = npgSqlConnection;
					npgSqlCommand.ExecuteNonQuery();
				}

				result.Success = true;
			}
			catch (Exception ex)
			{
				result.Message = ex.Message;
				result.Success = false;
			}

			return result;
		}
		#endregion

		#region Поиск в таблице по параметрам
		/// <summary>
		/// Поиск в таблице по параметрам
		/// </summary>
		/// <param name="tableName">Название таблицы</param>
		/// <param name="parameters">Параметры запроса</param>
		/// <returns>Результат SQL запроса</returns>
		public MethodResult FindByParams(string tableName, object parameters)
		{
			var result = new MethodResult();
			var objArray = new JArray();

			try
			{
				string[] param = ParseQueryParameters(parameters);

				using (var npgSqlConnection = new NpgsqlConnection(connectionString))
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

				result.Data = objArray.Count > 0 ? JsonConvert.SerializeObject(objArray) : null;
				result.Success = true;
			}
			catch (Exception ex)
			{
				result.Message = ex.Message;
				result.Success = false;
			}

			return result;
		}
		#endregion

		#region Загрузка файла с удаленного ресурса по Url
		/// <summary>
		/// Загрузка файла с удаленного ресурса по Url
		/// </summary>
		/// <param name="url">Url адрес представляющий собой ссылку на загрузку файла</param>
		/// <returns>MethodResult где свойство Data хранить локальный путь к загруженному файлу</returns>
		public MethodResult DownloadFile(string url)
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
						using (response = httpClient.SendAsync(request).Result)
						{
							var location = response.Headers.Location;
							if (location != null)
							{
								response = httpClient.GetAsync(location.OriginalString).Result;
								stream = response.Content.ReadAsStreamAsync().Result;
							}
							else
							{
								stream = response.Content.ReadAsStreamAsync().Result;
							}

							string mediaType = response.Content.Headers.ContentType.MediaType;
							string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Download");

							if (!Directory.Exists(directoryPath))
							{
								Directory.CreateDirectory(directoryPath);
							}

							string path = Path.Combine(directoryPath, Guid.NewGuid().ToString() + "." +
								mediaType.Substring(mediaType.IndexOf('/') + 1)
							);

							using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
							{
								stream.CopyTo(fs);
							}

							result.Data = path;
							result.Success = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				result.Message = ex.Message;
				result.Success = false;
			}

			return result;
		}
		#endregion

		#region Распаковка архива
		/// <summary>
		/// Распаковка архива, p.s тестировался только на .zip
		/// </summary>
		/// <param name="path">Локальный путь к архиву</param>
		/// <returns> MethodResult, где в свойстве Data находится список экземпляров FileModel 
		/// содержащих необходимую информацию о распакованных файлах
		/// </returns>
		public MethodResult UnpackArchive(string path)
		{
			var result = new MethodResult() { Success = false };
			var model = new List<FileModel>();

			try
			{
				using (var zip = ZipFile.OpenRead(path))
				{
					var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Files");

					if (!Directory.Exists(directoryPath))
					{
						Directory.CreateDirectory(directoryPath);
					}

					foreach (var entry in zip.Entries)
					{
						Guid localFileName = Guid.NewGuid();
						string filePath = Path.Combine(directoryPath, localFileName.ToString() + Path.GetExtension(entry.FullName));

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
			catch (Exception ex)
			{
				result.Message = ex.Message;
				result.Success = false;
			}

			return result;
		}
		#endregion

		public MethodResult UploadFile(string base64File, string fileName)
		{
			var result = new MethodResult();
			string filePath = string.Empty;

			try
			{
				string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Files");
				filePath = Path.Combine(directoryPath, fileName);

				if (!Directory.Exists(directoryPath))
					Directory.CreateDirectory(directoryPath);

				byte[] file = Convert.FromBase64String(base64File);
				File.WriteAllBytes(filePath, file);

				result.Data = filePath;
				result.Success = true;
			}
			catch (Exception ex)
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
