﻿using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace Greatbone.Core
{
    public class DbContext : IDisposable, ISource
    {
        readonly NpgsqlConnection connection;

        readonly NpgsqlCommand command;

        readonly DbParameters parameters;

        private NpgsqlTransaction transact;

        private NpgsqlDataReader reader;

        bool disposed;


        internal DbContext(NpgsqlConnectionStringBuilder builder)
        {
            connection = new NpgsqlConnection(builder);
            command = new NpgsqlCommand();
            parameters = new DbParameters(command.Parameters);
            command.Connection = connection;
        }

        public void Begin()
        {
            if (transact == null)
            {
                transact = connection.BeginTransaction();
                command.Transaction = transact;
            }
        }

        public void Begin(IsolationLevel level)
        {
            if (transact == null)
            {
                transact = connection.BeginTransaction(level);
                command.Transaction = transact;
            }
        }

        public void Commit()
        {
            if (transact != null)
            {
                transact.Commit();
                command.Transaction = null;
                transact = null;
            }
        }

        public void Rollback()
        {
            if (transact != null)
            {
                transact.Rollback();
                command.Transaction = null;
                transact = null;
            }
        }

        public bool QueryA(string cmdtext, Action<DbParameters> ps)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
            // setup command
            command.CommandText = cmdtext;
            command.CommandType = CommandType.Text;
            parameters.Clear();
            ps?.Invoke(parameters);
            ordinal = 0;
            reader = command.ExecuteReader();
            return reader.Read();
        }

        public bool Query(string cmdtext, Action<DbParameters> ps)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
            // setup command
            command.CommandText = cmdtext;
            command.CommandType = CommandType.Text;
            parameters.Clear();
            ps?.Invoke(parameters);
            ordinal = 0;
            reader = command.ExecuteReader();
            return reader.HasRows;
        }

        public bool NextRow()
        {
            ordinal = 0; // reset column ordinal

            if (reader == null)
            {
                return false;
            }
            return reader.Read();
        }

        public bool HasRows
        {
            get
            {
                if (reader == null)
                {
                    return false;
                }
                return reader.HasRows;
            }
        }

        public bool NextResult()
        {
            ordinal = 0; // reset column ordinal

            if (reader == null)
            {
                return false;
            }
            return reader.NextResult();
        }

        public int Execute(string cmdtext, Action<DbParameters> ps)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
            // setup command
            command.CommandText = cmdtext;
            command.CommandType = CommandType.Text;
            parameters.Clear();
            ps?.Invoke(parameters);
            ordinal = 0;
            return command.ExecuteNonQuery();
        }

        public object Scalar(string cmdtext, Action<DbParameters> ps)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
            // setup command
            command.CommandText = cmdtext;
            command.CommandType = CommandType.Text;
            parameters.Clear();
            ps?.Invoke(parameters);
            ordinal = 0;
            return command.ExecuteScalar();
        }

        //
        // RESULTSET
        //

        public T ToObj<T>(uint x = 0) where T : IPersist, new()
        {
            T obj = new T();
            obj.Load(this, x);
            return obj;
        }


        public T[] ToArr<T>(uint x = 0) where T : IPersist, new()
        {
            List<T> lst = new List<T>(64);
            while (NextRow())
            {
                T obj = new T();
                obj.Load(this, x);
                lst.Add(obj);
            }
            return lst.ToArray();
        }


        // current column ordinal
        int ordinal;

        public bool Got(string name, ref bool v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                v = reader.GetBoolean(ord);
                return true;
            }
            return false;
        }

        public bool Got(string name, ref short v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                v = reader.GetInt16(ord);
                return true;
            }
            return false;
        }

        public bool Got(string name, ref int v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                v = reader.GetInt32(ord);
                return true;
            }
            return false;
        }

        public bool Got(string name, ref long v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                v = reader.GetInt64(ord);
                return true;
            }
            return false;
        }

        public bool Got(string name, ref decimal v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                v = reader.GetDecimal(ord);
                return true;
            }
            return false;
        }

        public bool Got(string name, ref Number v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                decimal decv = reader.GetDecimal(ord);
                // TODO
                return true;
            }
            return false;
        }

        public bool Got(string name, ref DateTime v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                v = reader.GetDateTime(ord);
                return true;
            }
            return false;
        }

        public bool Got(string name, ref char[] v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                v = reader.GetFieldValue<char[]>(ord);
                return true;
            }
            return false;
        }

        public bool Got(string name, ref string v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                v = reader.GetString(ord);
                return true;
            }
            return false;
        }

        public bool Got(string name, ref byte[] v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            try
            {
                if (!reader.IsDBNull(ord))
                {
                    int len;
                    if ((len = (int)reader.GetBytes(ord, 0, null, 0, 0)) > 0)
                    {
                        // get the number of bytes that are available to read.
                        v = new byte[len];
                        reader.GetBytes(ord, 0, v, 0, len); // read data into the buffer
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public bool Got(string name, ref ArraySegment<byte> v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            try
            {
                if (!reader.IsDBNull(ord))
                {
                    int len;
                    if ((len = (int)reader.GetBytes(ord, 0, null, 0, 0)) > 0)
                    {
                        byte[] arr = BufferPool.Borrow(len);
                        reader.GetBytes(ord, 0, arr, 0, len); // read data into the buffer
                        v = new ArraySegment<byte>(arr, 0, len);
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public bool Got<V>(string name, ref V v, uint x = 0) where V : IPersist, new()
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                string str = reader.GetString(ord);
                JTextParse parse = new JTextParse(str);
                JObj jo = (JObj)parse.Parse();
                v = new V();
                v.Load(jo, x);
                return true;
            }
            return false;
        }

        public bool Got(string name, ref JObj v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                string str = reader.GetString(ord);
                JTextParse parse = new JTextParse(str);
                v = (JObj)parse.Parse();
                return true;
            }
            return false;
        }

        public bool Got(string name, ref JArr v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                string str = reader.GetString(ord);
                JTextParse parse = new JTextParse(str);
                v = (JArr)parse.Parse();
                return true;
            }
            return false;
        }

        public bool Got(string name, ref short[] v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                v = reader.GetFieldValue<short[]>(ord);
                return true;
            }
            return false;
        }

        public bool Got(string name, ref int[] v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                v = reader.GetFieldValue<int[]>(ord);
                return true;
            }
            return false;
        }

        public bool Got(string name, ref long[] v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                v = reader.GetFieldValue<long[]>(ord);
                return true;
            }
            return false;
        }

        public bool Got(string name, ref string[] v)
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                v = reader.GetFieldValue<string[]>(ord);
                return true;
            }
            return false;
        }

        public bool Got<V>(string name, ref V[] v, uint x = 0) where V : IPersist, new()
        {
            int ord = name == null ? ordinal++ : reader.GetOrdinal(name);
            if (!reader.IsDBNull(ord))
            {
                string str = reader.GetString(ord);
                JTextParse parse = new JTextParse(str);
                JArr ja = (JArr)parse.Parse();
                int len = ja.Count;
                v = new V[len];
                for (int i = 0; i < len; i++)
                {
                    JObj jo = (JObj)ja[i];
                    V obj = new V();
                    obj.Load(jo, x);
                    v[i] = obj;
                }
                return true;
            }
            return false;
        }


        //
        // MESSAGING
        //

        public void Publish(string topic, string part, Action<JContent> a)
        {
            // convert message to byte buffer
            JContent jcont = new JContent(8 * 1024);
            a?.Invoke(jcont);

            Execute("INSERT INTO mq (topic, filter, message) VALUES (@topic, @filter, @message)", p =>
            {
                p.Put("@topic", topic);
                p.Put("@filter", part);
                // p.Put("@message", new ArraySegment<byte>(b.Buffer, 0, b.Length));
            });
        }

        public void Publish<T>(string topic, string part, T obj) where T : IPersist
        {
            Publish(topic, part, jcont => jcont.PutObj(obj));
        }

        public void Publish<T>(string topic, string part, T[] arr) where T : IPersist
        {
            Publish(topic, part, jcont => jcont.PutArr(arr));
        }


        public void Dispose()
        {
            if (!disposed)
            {
                reader?.Dispose();
                command.Dispose();
                connection.Dispose();
                // indicate that the instance has been disposed.
                disposed = true;
            }
        }

    }
}