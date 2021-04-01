using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace Templar.Database
{
    public class Database<T> where T : IDatabaseModal, new()
    {
        public string TableName { get; }

        public Database(string tableName)
        {
            TableName = tableName;
        }

        public async Task<int> DeleteByProperty(string propName, object value)
        {
            return ExecuteQuery($@"DELETE FROM {TableName} WHERE {propName}=@p0", value);
        }

        public async Task<List<T>> GetByProperty(string propName, object value)
        {
            var toReturn = new List<T>();
            (var q, var conn) = RequestQuery($"SELECT * FROM {TableName} WHERE {propName}=@p0;", value);
            while (q.Read())
            {
                toReturn.Add(await Convert(q));
            }
            q.Dispose();
            conn.Dispose();
            return toReturn;
        }

        public async Task<List<T>> GetAll()
        {
            var toReturn = new List<T>();
            (var q, var conn) = RequestQuery($"SELECT * FROM {TableName};");
            while (q.Read())
            {
                toReturn.Add(await Convert(q));
            }
            q.Dispose();
            conn.Dispose();
            return toReturn;
        }

        public async Task Insert(T t)
        {
            (var query, var objs) = await Convert(t);

            query = $"INSERT INTO {TableName} {query};";
            ExecuteQuery(query, objs);
        }

        internal static async Task<T> Convert(MySqlDataReader r)
        {
            var t = new T();
            var fields = typeof(T).GetFields(BindingFlags.Public);

            foreach (var column in await r.GetColumnSchemaAsync())
            {
                var field = fields.FirstOrDefault(f => f.Name == column.ColumnName);
                if (field is null) { continue; }

                var v = r[field.Name];
                field.SetValue(t, v);
            }

            return t;
        }

        internal static async Task<(string, object[])> Convert(T t)
        {
            var fields = typeof(T).GetFields();
            var objs = fields.Select(f =>
            {
                return f.GetValue(t);
            }).ToArray();

            var nums = string.Join(", ", Enumerable.Range(0, objs.Length).Select(i => $"@p{i}"));
            var toReturn = "(" + string.Join(", ", fields.Select(f => f.Name)) + ") VALUES (" + nums + ")";

            return (toReturn, objs);
        }

        internal static int ExecuteQuery(string s, params object[] args)
        {
            using var conn = new MySqlConnection(Bot.DatabaseConnString);
            try
            {
                conn.Open();
            } catch { /* error log here */ return -1; }

            try
            {
                return GetCommand(s, args, conn).ExecuteNonQuery();
            } catch  { /* error log here */ return -2; }
        }

        internal static (MySqlDataReader, MySqlConnection) RequestQuery(string s, params object[] args)
        {
            try
            {
                var conn = new MySqlConnection(Bot.DatabaseConnString);
                conn.Open();
                var r = GetCommand(s, args, conn).ExecuteReader();
                return (r, conn);
            } catch { return default; }
        }

        internal static MySqlCommand GetCommand(string s, object[] args, MySqlConnection conn)
        {
            s = s.Trim();
            s = s.EndsWith(";") ? s : s + ";";
            var cmd = new MySqlCommand(s, conn);
            for (var i = 0; i < args.Length; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", args[i]);
            }
            return cmd;
        }
    }
}
