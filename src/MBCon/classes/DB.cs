using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MySql.Data.MySqlClient;
using Proxy;
using Proxy.DB;

namespace MBCon.classes
{
    public class DB : IDB
    {

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public Result Execute(Query query)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = null;
            var result = new Result();

            try
            {
                connection = new MySqlConnection(String.Format(
                    @"server={0};userid={1};password={2};database={3}",
                    query.Conn.Host,
                    query.Conn.User,
                    query.Conn.Password,
                    query.Conn.Database
                ));
                connection.Open();

                var sql = query.Sql;
                var command = new MySqlCommand(sql, connection);

                foreach (var param in query.Params)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }

                if (query.QueryType != Query.Type.Select)
                {
                    command.ExecuteNonQuery();
                    result.NoResult = true;
                }
                else
                {
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var line = new Dictionary<string, string>();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var val = (!reader.IsDBNull(i)) ? reader.GetString(i) : "";
                            line.Add(reader.GetName(i), val);
                        }
                        result.Rows.Add(line);
                    }
                }
            }
            catch (MySqlException ex)
            {
                AppConsole.Log(String.Format("MYSQL ERROR: {0}", ex.Message));
                result.Error = true;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                if (connection != null)
                {
                    connection.Close();
                }
            }
            return result;
        }

    }
}
