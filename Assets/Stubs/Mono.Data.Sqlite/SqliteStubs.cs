// Stub implementations for Mono.Data.SqliteClient (SQLite database access).
// These stubs exist solely so the Roslyn analyzer project compiles.
// THESE TYPES MUST NOT BE USED IN RUNTIME CODE.

using System;
using System.Collections;
using System.Data;

namespace Mono.Data.SqliteClient
{
    public class SqliteConnection : IDisposable
    {
        public SqliteConnection(string connectionString) { }
        public string ConnectionString { get; set; }
        public ConnectionState State => ConnectionState.Closed;
        public void Open() { }
        public void Close() { }
        public void Dispose() { }
        public SqliteCommand CreateCommand() => new SqliteCommand(this);
    }

    public class SqliteCommand : IDisposable
    {
        public SqliteCommand(SqliteConnection connection) { }
        public SqliteCommand(string commandText, SqliteConnection connection) { }
        public string CommandText { get; set; }
        public SqliteConnection Connection { get; set; }
        public SqliteParameterCollection Parameters => new SqliteParameterCollection();
        public SqliteDataReader ExecuteReader() => new SqliteDataReader();
        public object ExecuteScalar() => null;
        public int ExecuteNonQuery() => 0;
        public void Dispose() { }
    }

    public class SqliteDataReader : IDisposable, IEnumerable
    {
        public bool Read() => false;
        public bool NextResult() => false;
        public void Close() { }
        public void Dispose() { }
        public int FieldCount => 0;
        public string GetName(int i) => string.Empty;
        public object GetValue(int i) => null;
        public bool IsDBNull(int i) => true;
        public string GetString(int i) => string.Empty;
        public int GetInt32(int i) => 0;
        public long GetInt64(int i) => 0L;
        public float GetFloat(int i) => 0f;
        public double GetDouble(int i) => 0.0;
        public bool GetBoolean(int i) => false;
        public byte GetByte(int i) => 0;
        public int GetOrdinal(string name) => 0;
        public object this[int i] => null;
        public object this[string name] => null;
        public IEnumerator GetEnumerator() => System.Linq.Enumerable.Empty<object>().GetEnumerator();
    }

    public class SqliteParameterCollection
    {
        public void Add(string parameterName, object value) { }
        public void AddWithValue(string parameterName, object value) { }
    }

    public class SqliteException : Exception
    {
        public SqliteException(string message) : base(message) { }
        public SqliteException(string message, Exception inner) : base(message, inner) { }
    }

    public class SqliteSyntaxException : Exception
    {
        public SqliteSyntaxException(string message) : base(message) { }
    }
}
