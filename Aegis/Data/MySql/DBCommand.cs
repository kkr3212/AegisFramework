using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Aegis.Threading;



namespace Aegis.Data.MySql
{
    public sealed class DBCommand : IDisposable
    {
        private readonly MySqlDatabase _mysql;
        private readonly MySqlCommand _command = new MySqlCommand();
        private DBConnector _dbConnector;
        private Boolean _isAsync;
        private List<Tuple<String, Object>> _prepareBindings = new List<Tuple<String, Object>>();

        public StringBuilder CommandText { get; } = new StringBuilder(256);
        public DataReader Reader { get; private set; }
        public Int32 CommandTimeout { get { return _command.CommandTimeout; } set { _command.CommandTimeout = value; } }
        public Int64 LastInsertedId { get { return _command?.LastInsertedId ?? 0; } }
        public Object Tag { get; set; }





        public DBCommand(MySqlDatabase mysql, Int32 timeoutSec = 60)
        {
            _mysql = mysql;
            _isAsync = false;
            CommandTimeout = timeoutSec;
        }


        public void Dispose()
        {
            //  비동기로 동작중인 쿼리는 작업이 끝나기 전에 반환할 수 없다.
            if (_isAsync == true)
                return;

            EndQuery();
            _command.Dispose();
        }


        public void QueryNoReader()
        {
            if (_dbConnector != null || Reader != null)
                throw new AegisException(AegisResult.DataReaderNotClosed, "There is already an open DataReader associated with this Connection which must be closed first.");


            _dbConnector = _mysql.GetDBC();
            _command.Connection = _dbConnector.Connection;
            _command.CommandText = CommandText.ToString();

            Prepare();
            _command.ExecuteNonQuery();
            _dbConnector.QPS.Add(1);
            EndQuery();
        }


        public DataReader Query()
        {
            if (_dbConnector != null || Reader != null)
                throw new AegisException(AegisResult.DataReaderNotClosed, "There is already an open DataReader associated with this Connection which must be closed first.");


            _dbConnector = _mysql.GetDBC();
            _command.Connection = _dbConnector.Connection;
            _command.CommandText = CommandText.ToString();

            Prepare();
            Reader = new DataReader(_command.ExecuteReader());
            _dbConnector.QPS.Add(1);

            return Reader;
        }


        public void QueryNoReader(String query, params object[] args)
        {
            CommandText.Clear();
            CommandText.AppendFormat(query, args);
            QueryNoReader();
        }


        public DataReader Query(String query, params object[] args)
        {
            CommandText.Clear();
            CommandText.AppendFormat(query, args);
            return Query();
        }


        public void PostQueryNoReader()
        {
            _isAsync = true;
            SpinWorker.Work(() =>
            {
                try
                {
                    QueryNoReader();

                    _isAsync = false;
                    Dispose();
                }
                catch (Exception)
                {
                    _isAsync = false;
                    Dispose();
                    throw;  //  상위 Exception Handler가 처리하도록 예외를 던진다.
                }
            });
        }


        public void PostQueryNoReader(Action<Exception> actionOnCompletion)
        {
            Exception exception = null;

            _isAsync = true;
            SpinWorker.Work(() =>
            {
                try
                {
                    QueryNoReader();

                    _isAsync = false;
                    Dispose();
                }
                catch (Exception e)
                {
                    exception = e;
                    _isAsync = false;
                    Dispose();
                }
            },
            () => { actionOnCompletion(exception); });
        }


        public void PostQuery(Action actionOnRead, Action<Exception> actionOnCompletion)
        {
            Exception exception = null;

            _isAsync = true;
            SpinWorker.Work(() =>
            {
                try
                {
                    Query();
                    if (actionOnRead != null)
                        actionOnRead();

                    _isAsync = false;
                    Dispose();
                }
                catch (Exception e)
                {
                    exception = e;
                    _isAsync = false;
                    Dispose();
                    throw;  //  상위 Exception Handler가 처리하도록 예외를 던진다.
                }
            },
            () => { actionOnCompletion(exception); });
        }


        public void PostQuery(Action<DBCommand> actionOnRead, Action<Exception> actionOnCompletion)
        {
            Exception exception = null;

            _isAsync = true;
            SpinWorker.Work(() =>
            {
                try
                {
                    Query();
                    if (actionOnRead != null)
                        actionOnRead(this);

                    _isAsync = false;
                    Dispose();
                }
                catch (Exception e)
                {
                    exception = e;
                    _isAsync = false;
                    Dispose();
                    throw;  //  상위 Exception Handler가 처리하도록 예외를 던진다.
                }
            },
            () => { actionOnCompletion(exception); });
        }


        private void Prepare()
        {
            if (_prepareBindings.Count() == 0)
                return;

            _command.Prepare();
            foreach (Tuple<String, Object> param in _prepareBindings)
                _command.Parameters.AddWithValue(param.Item1, param.Item2);
        }


        public void BindParameter(String parameterName, object value)
        {
            _prepareBindings.Add(new Tuple<String, Object>(parameterName, value));
        }


        public void EndQuery()
        {
            CommandText.Clear();
            _prepareBindings.Clear();
            _command.Parameters.Clear();
            _command.Connection = null;

            if (Reader != null)
            {
                Reader.Dispose();
                Reader = null;
            }

            if (_dbConnector != null)
            {
                _mysql.ReturnDBC(_dbConnector);
                _dbConnector = null;
            }
        }
    }
}
