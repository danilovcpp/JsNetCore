using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace JsNetCore.Models
{
    public class Context
    {
        private string connectionString = "Server=localhost;Port=5432;User Id = postgres; Password=123;Database=ownRadio;";

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

        public string ExecSqlQuery(string query)
        {
            //TODO: брать путь из бд
            return @"D:\Work\TestFile.txt";
        }

        /// <summary>
        /// Метод получения содержимого файла в виде байт массива
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <returns>Массив байт</returns>
        public byte[] LoadFile(string path)
        {
            string textFromFile = string.Empty;
            byte[] array;

            if (!File.Exists(path))
                return null;

            using (FileStream fstream = File.OpenRead(path))
            {
                // преобразуем строку в байты
                array = new byte[fstream.Length];

                // считываем данные
                fstream.Read(array, 0, array.Length);

                // декодируем байты в строку
                textFromFile = Encoding.Default.GetString(array);
            }

            return array;
        }


        public string GetById(string id)
        {
            /*
            var obj = new JObject();

            using (var npgSqlConnection = new NpgsqlConnection(connectionString))
            {
                var npgSqlCommand = new NpgsqlCommand();
                npgSqlCommand.CommandText = $"SELECT \"recId\", \"recName\", \"artist\" WHERE \"recId\" = 'b9d27749-2f0f-4bca-aa3a-c0685e6235d9'";
                npgSqlConnection.Open();
                npgSqlCommand.Connection = npgSqlConnection;

                NpgsqlDataReader reader = npgSqlCommand.ExecuteReader();
                if (reader.Read())
                {
                    obj.Add("recId", (JObject)reader[0]);
                    obj.Add("recName", (JObject)reader[1]);
                    obj.Add("artist", (JObject)reader[2]);
                }
            }*/


            //TODO: изменить на получение значения из БД по uuid
            var obj = new JObject();

            obj.Add("recId", Guid.NewGuid().ToString());
            obj.Add("recName", $"Track №1");
            obj.Add("artist", $"Artist №1");

            return JsonConvert.SerializeObject(obj);
        }

        public void Insert(object @object)
        {
            //TODO: уйти от хард кода в запросе и параметрах
            using (var npgSqlConnection = new NpgsqlConnection(connectionString))
            {
                var npgSqlCommand = new NpgsqlCommand();
                npgSqlCommand.CommandText = $"INSERT INTO tracks(recId, \"recName\", artist) VALUES(@guid, @trackName, @artist)";

                npgSqlCommand.Parameters.AddWithValue("@guid", Guid.NewGuid());
                npgSqlCommand.Parameters.AddWithValue("@trackName", "Track1");
                npgSqlCommand.Parameters.AddWithValue("@artist", "Atrist1");
                npgSqlConnection.Open();
                npgSqlCommand.Connection = npgSqlConnection;
                npgSqlCommand.ExecuteNonQuery();
            }
        }

        public void Update(string id, object @object)
        {
            string name = "Template";
            string artist = "ArtistTemplate";

            //TODO: аналогично insert методу
            using (var npgSqlConnection = new NpgsqlConnection(connectionString))
            {
                var npgSqlCommand = new NpgsqlCommand();
                npgSqlCommand.CommandText = $"UPDATE tracks SET recName = {name}, artist = {artist} WHERE recId = {id};";
                npgSqlConnection.Open();
                npgSqlCommand.Connection = npgSqlConnection;
                npgSqlCommand.ExecuteNonQuery();
            }
        }
    }
}
