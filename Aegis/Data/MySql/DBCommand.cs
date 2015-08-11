using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;



namespace Aegis.Data.MySql
{
    public sealed class DBCommand : IDisposable
    {
        private MySqlDatabase _mysql;
        private DBConnector _connector;
        private MySqlCommand _cmd;
        private DataReader _reader;
        private Boolean _isAsync;
        private List<Tuple<String, Object>> _prepareBindings;

        public StringBuilder CommandText { get; set; }
        public Int32 CommandTimeout { get { return _cmd.CommandTimeout; } set { _cmd.CommandTimeout = value; } }
        public Int64 LastInsertedId
        {
            get
            {
                if (_cmd == null)
                    return 0;

                return _cmd.LastInsertedId;
            }
        }





        private DBCommand()
        {
            CommandText = new StringBuilder(256);
            _prepareBindings = new List<Tuple<String, Object>>();
            _cmd = new MySqlCommand();
        }


        public static DBCommand NewCommand(MySqlDatabase mysql, Int32 timeoutSec = 60)
        {
            DBCommand obj = ObjectPool<DBCommand>.Pop();
            obj._mysql = mysql;
            obj._connector = null;
            obj._isAsync = false;
            obj.CommandTimeout = timeoutSec;

            return obj;
        }


        internal static DBCommand NewCommand(DBConnector conn, Int32 timeoutSec = 60)
        {
            DBCommand obj = ObjectPool<DBCommand>.Pop();
            obj._mysql = null;
            obj._connector = conn;
            obj._isAsync = false;
            obj.CommandTimeout = timeoutSec;

            return obj;
        }


        public void Dispose()
        {
            //  비동기로 동작중인 쿼리는 작업이 끝나기 전에 반환할 수 없다.
            if (_isAsync == true)
                return;


            EndQuery();

            _mysql = null;
            _connector = null;
            _cmd.Connection = null;
            ObjectPool<DBCommand>.Push(this);
        }


        public void QueryNoReader()
        {
            try
            {
                if (_connector != null)
                {
                    _cmd.Connection = _connector.Connection;
                    _cmd.CommandText = CommandText.ToString();

                    Prepare();
                    _cmd.ExecuteNonQuery();

                    _connector.IncreaseQueryCount();
                }
                else
                {
                    using (DBConnector dbc = _mysql.GetDBC())
                    {
                        _cmd.Connection = dbc.Connection;
                        _cmd.CommandText = CommandText.ToString();

                        Prepare();
                        _cmd.ExecuteNonQuery();
                        _cmd.Connection = null;

                        dbc.IncreaseQueryCount();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, _cmd.CommandText);
                Logger.Write(LogType.Err, 1, e.ToString());
            }
        }


        public DataReader Query()
        {
            try
            {
                if (_connector != null)
                {
                    if (_reader != null)
                        _reader.Close();

                    _cmd.Connection = _connector.Connection;
                    _cmd.CommandText = CommandText.ToString();

                    Prepare();

                    _reader = new DataReader(_cmd.ExecuteReader());
                    _connector.IncreaseQueryCount();
                }
                else
                {
                    using (DBConnector dbc = _mysql.GetDBC())
                    {
                        if (_reader != null)
                            _reader.Close();

                        _cmd.Connection = dbc.Connection;
                        _cmd.CommandText = CommandText.ToString();

                        Prepare();

                        _reader = new DataReader(_cmd.ExecuteReader());
                        _cmd.Connection = null;

                        dbc.IncreaseQueryCount();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, _cmd.CommandText);
                Logger.Write(LogType.Err, 1, e.ToString());
            }

            return _reader;
        }


        public void QueryNoReader(String query, params object[] args)
        {
            CommandText.Clear();
            CommandText.AppendFormat(query, args);

            Query();
        }


        public DataReader Query(String query, params object[] args)
        {
            CommandText.Clear();
            CommandText.AppendFormat(query, args);

            return Query();
        }


        public void QueryAsync()
        {
            _isAsync = true;
            _mysql.WorkerQueue.Post(() =>
            {
                QueryNoReader();
                _isAsync = false;
                Dispose();
            });
        }


        private void Prepare()
        {
            if (_prepareBindings.Count() == 0)
                return;

            _cmd.Prepare();
            foreach (Tuple<String, Object> param in _prepareBindings)
                _cmd.Parameters.AddWithValue(param.Item1, param.Item2);
        }


        public void BindParameter(String parameterName, object value)
        {
            _prepareBindings.Add(new Tuple<String, Object>(parameterName, value));
        }


        public void EndQuery()
        {
            CommandText.Clear();
            _prepareBindings.Clear();
            _cmd.Parameters.Clear();
            _cmd.Connection = null;

            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }
        }
    }
}
