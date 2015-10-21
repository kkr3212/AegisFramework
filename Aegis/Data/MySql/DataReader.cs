using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;
using MySql.Data.Types;
using MySql.Data.MySqlClient;



namespace Aegis.Data.MySql
{
    public sealed class DataReader : DbDataReader, IDataReader, IDisposable, IDataRecord
    {
        private readonly MySqlDataReader _reader;

        public override int Depth { get { return _reader.Depth; } }
        public override int FieldCount { get { return _reader.FieldCount; } }
        public override bool HasRows { get { return _reader.HasRows; } }
        public override bool IsClosed { get { return _reader.IsClosed; } }
        public override int RecordsAffected { get { return _reader.RecordsAffected; } }

        public override object this[int i] { get { return _reader[i]; } }
        public override object this[string name] { get { return _reader[name]; } }





        public DataReader(MySqlDataReader reader)
        {
            _reader = reader;
        }

        public override void Close()
        {
            _reader.Close();
        }


        public new void Dispose()
        {
            _reader.Dispose();
            base.Dispose();
        }


        public override bool GetBoolean(int i)
        {
            return _reader.GetBoolean(i);
        }


        public bool GetBoolean(string name)
        {
            return _reader.GetBoolean(name);
        }


        public override byte GetByte(int i)
        {
            return _reader.GetByte(i);
        }


        public byte GetByte(string name)
        {
            return _reader.GetByte(name);
        }


        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return _reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }


        public override char GetChar(int i)
        {
            return _reader.GetChar(i);
        }


        public char GetChar(string name)
        {
            return _reader.GetChar(name);
        }


        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return _reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }


        public override string GetDataTypeName(int i)
        {
            return _reader.GetDataTypeName(i);
        }


        public override DateTime GetDateTime(int i)
        {
            return _reader.GetDateTime(i);
        }


        public DateTime GetDateTime(string column)
        {
            return _reader.GetDateTime(column);
        }


        public override decimal GetDecimal(int i)
        {
            return _reader.GetDecimal(i);
        }


        public decimal GetDecimal(string column)
        {
            return _reader.GetDecimal(column);
        }


        public override double GetDouble(int i)
        {
            return _reader.GetDouble(i);
        }


        public double GetDouble(string column)
        {
            return _reader.GetDouble(column);
        }


        public override IEnumerator GetEnumerator()
        {
            return _reader.GetEnumerator();
        }


        public override Type GetFieldType(int i)
        {
            return _reader.GetFieldType(i);
        }


        public Type GetFieldType(string column)
        {
            return _reader.GetFieldType(column);
        }


        public override float GetFloat(int i)
        {
            return _reader.GetFloat(i);
        }


        public float GetFloat(string column)
        {
            return _reader.GetFloat(column);
        }


        public override Guid GetGuid(int i)
        {
            return _reader.GetGuid(i);
        }


        public Guid GetGuid(string column)
        {
            return _reader.GetGuid(column);
        }


        public override short GetInt16(int i)
        {
            return _reader.GetInt16(i);
        }


        public short GetInt16(string column)
        {
            return _reader.GetInt16(column);
        }


        public override int GetInt32(int i)
        {
            return _reader.GetInt32(i);
        }


        public int GetInt32(string column)
        {
            return _reader.GetInt32(column);
        }


        public override long GetInt64(int i)
        {
            return _reader.GetInt64(i);
        }


        public long GetInt64(string column)
        {
            return _reader.GetInt64(column);
        }


        public MySqlDateTime GetMySqlDateTime(int column)
        {
            return _reader.GetMySqlDateTime(column);
        }


        public MySqlDateTime GetMySqlDateTime(string column)
        {
            return _reader.GetMySqlDateTime(column);
        }


        public MySqlDecimal GetMySqlDecimal(int i)
        {
            return _reader.GetMySqlDecimal(i);
        }


        public MySqlDecimal GetMySqlDecimal(string column)
        {
            return _reader.GetMySqlDecimal(column);
        }


        public MySqlGeometry GetMySqlGeometry(int i)
        {
            return _reader.GetMySqlGeometry(i);
        }


        public MySqlGeometry GetMySqlGeometry(string column)
        {
            return _reader.GetMySqlGeometry(column);
        }


        public override string GetName(int i)
        {
            return _reader.GetName(i);
        }


        public override int GetOrdinal(string name)
        {
            return _reader.GetOrdinal(name);
        }


        public sbyte GetSByte(int i)
        {
            return _reader.GetSByte(i);
        }


        public sbyte GetSByte(string name)
        {
            return _reader.GetSByte(name);
        }


        public override DataTable GetSchemaTable()
        {
            return _reader.GetSchemaTable();
        }


        public override string GetString(int i)
        {
            return _reader.GetString(i);
        }


        public string GetString(string column)
        {
            return _reader.GetString(column);
        }


        public TimeSpan GetTimeSpan(int column)
        {
            return _reader.GetTimeSpan(column);
        }


        public TimeSpan GetTimeSpan(string column)
        {
            return _reader.GetTimeSpan(column);
        }


        public ushort GetUInt16(int column)
        {
            return _reader.GetUInt16(column);
        }


        public ushort GetUInt16(string column)
        {
            return _reader.GetUInt16(column);
        }


        public uint GetUInt32(int column)
        {
            return _reader.GetUInt32(column);
        }


        public uint GetUInt32(string column)
        {
            return _reader.GetUInt32(column);
        }


        public ulong GetUInt64(int column)
        {
            return _reader.GetUInt64(column);
        }


        public ulong GetUInt64(string column)
        {
            return _reader.GetUInt64(column);
        }


        public override object GetValue(int i)
        {
            return _reader.GetValue(i);
        }


        public override int GetValues(object[] values)
        {
            return _reader.GetValues(values);
        }


        public override bool IsDBNull(int i)
        {
            return _reader.IsDBNull(i);
        }


        public override bool NextResult()
        {
            return _reader.NextResult();
        }


        public override bool Read()
        {
            return _reader.Read();
        }
    }
}
