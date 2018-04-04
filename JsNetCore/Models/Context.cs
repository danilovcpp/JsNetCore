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
using System.Diagnostics;
using System.Data;

namespace JsNetCore.Models
{
    public class Context
    {
        private string connectionString = "Server=localhost;Port=5432;User Id = postgres; Password=123;Database=ownRadioRdev;";

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
        public string FindById(string tableName, string id)
        {
            var obj = new JObject();

            id = id.Replace("'", "");

            using (var npgSqlConnection = new NpgsqlConnection(connectionString))
            {
                var npgSqlCommand = new NpgsqlCommand($"SELECT * FROM {tableName} WHERE recid = '{id}'");

                try
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
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
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
        /// <returns></returns>
        public bool Insert(string tableName, object parameters)
        {
            var par = JObject.FromObject(parameters);
            Dictionary<string, string> param = new Dictionary<string, string>();

            foreach (var x in par)
            {
                param.Add(x.Key.ToLower(), $"'{x.Value.ToString()}'");
            }

            string cmdParams = String.Join(',', param.Keys);
            string cmdArgs = String.Join(',', param.Values);

            if (FindById(tableName, param["recid"]) != "")
                return false;

            using (var npgSqlConnection = new NpgsqlConnection(connectionString))
            {
                var npgSqlCommand = new NpgsqlCommand($"INSERT INTO {tableName}({cmdParams}) VALUES({cmdArgs})");

                try
                {
                    npgSqlConnection.Open();
                    npgSqlCommand.Connection = npgSqlConnection;
                    npgSqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    return false;
                }
            }

            return true;
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
    }
}
