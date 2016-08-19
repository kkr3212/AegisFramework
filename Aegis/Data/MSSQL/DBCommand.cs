using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Aegis.Threading;



namespace Aegis.Data.MSSQL
{
    public sealed class DBCommand : IDisposable
    {
        private readonly ConnectionPool _pool;
        private readonly SqlCommand _command = new SqlCommand();
        private DBConnector _dbConnector;
        private bool _isAsync;
        private List<Tuple<string, object>> _prepareBindings = new List<Tuple<string, object>>();

        public StringBuilder CommandText { get; } = new StringBuilder(256);
        public SqlDataReader Reader { get; private set; }
        public int CommandTimeout { get { return _command.CommandTimeout; } set { _command.CommandTimeout = value; } }
        public long LastInsertedId { get { return (long)_command.ExecuteScalar(); } }
        public object Tag { get; set; }





        public DBCommand(ConnectionPool pool, int timeoutSec = 60)
        {
            _pool = pool;
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


            _dbConnector = _pool.GetDBC();
            _command.Connection = _dbConnector.Connection;
            _command.CommandText = CommandText.ToString();

            Prepare();
            _command.ExecuteNonQuery();
            _dbConnector.QPS.Add(1);
            EndQuery();
        }


        public SqlDataReader Query()
        {
            if (_dbConnector != null || Reader != null)
                throw new AegisException(AegisResult.DataReaderNotClosed, "There is already an open DataReader associated with this Connection which must be closed first.");


            _dbConnector = _pool.GetDBC();
            _command.Connection = _dbConnector.Connection;
            _command.CommandText = CommandText.ToString();

            Prepare();
            Reader = _command.ExecuteReader();
            _dbConnector.QPS.Add(1);

            return Reader;
        }


        public void QueryNoReader(string query, params object[] args)
        {
            CommandText.Clear();
            CommandText.AppendFormat(query, args);
            QueryNoReader();
        }


        public SqlDataReader Query(string query, params object[] args)
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
            () => { actionOnCompletion?.Invoke(exception); });
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
                    actionOnRead?.Invoke();

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
            () => { actionOnCompletion?.Invoke(exception); });
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
                    actionOnRead?.Invoke(this);

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
            () => { actionOnCompletion?.Invoke(exception); });
        }


        private void Prepare()
        {
            if (_prepareBindings.Count() == 0)
                return;

            _command.Prepare();
            foreach (Tuple<string, object> param in _prepareBindings)
                _command.Parameters.AddWithValue(param.Item1, param.Item2);
        }


        public void BindParameter(string parameterName, object value)
        {
            _prepareBindings.Add(new Tuple<string, object>(parameterName, value));
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
                _pool.ReturnDBC(_dbConnector);
                _dbConnector = null;
            }
        }
    }
}
