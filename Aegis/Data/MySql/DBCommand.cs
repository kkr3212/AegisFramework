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
        private readonly MySqlDatabase _mysql;
        private readonly MySqlCommand _command = new MySqlCommand();
        private DBConnector _dbConnector;
        private Boolean _isAsync;
        private List<Tuple<String, Object>> _prepareBindings = new List<Tuple<String, Object>>();

        public StringBuilder CommandText { get; } = new StringBuilder(256);
        public DataReader Reader { get; private set; }
        public Int32 CommandTimeout { get { return _command.CommandTimeout; } set { _command.CommandTimeout = value; } }
        public Int64 LastInsertedId { get { return _command?.LastInsertedId ?? 0; } }





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


        public void Query()
        {
            if (_dbConnector != null || Reader != null)
                throw new AegisException(AegisResult.DataReaderNotClosed, "There is already an open DataReader associated with this Connection which must be closed first.");


            _dbConnector = _mysql.GetDBC();
            _command.Connection = _dbConnector.Connection;
            _command.CommandText = CommandText.ToString();

            Prepare();
            Reader = new DataReader(_command.ExecuteReader());
            _dbConnector.QPS.Add(1);
        }


        public void QueryNoReader(String query, params object[] args)
        {
            CommandText.Clear();
            CommandText.AppendFormat(query, args);
            QueryNoReader();
        }


        public void Query(String query, params object[] args)
        {
            CommandText.Clear();
            CommandText.AppendFormat(query, args);
            Query();
        }


        public void PostQueryNoReader()
        {
            _isAsync = true;
            SpinWorker.PostWork(() =>
            {
                try
                {
                    QueryNoReader();
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, CommandText.ToString());
                    Logger.Write(LogType.Err, 1, e.ToString());
                }

                _isAsync = false;
                Dispose();
            });
        }


        public void PostQueryNoReader(Action actionOnCompletion)
        {
            _isAsync = true;
            SpinWorker.PostWork(() =>
            {
                try
                {
                    Query();
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, CommandText.ToString());
                    Logger.Write(LogType.Err, 1, e.ToString());
                }
            },
            () =>
            {
                try
                {
                    actionOnCompletion();
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, e.ToString());
                }

                _isAsync = false;
                Dispose();
            });
        }


        public void PostQuery(Action actionOnCompletion)
        {
            _isAsync = true;
            SpinWorker.PostWork(() =>
            {
                try
                {
                    Query();
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, CommandText.ToString());
                    Logger.Write(LogType.Err, 1, e.ToString());
                }
            },
            () =>
            {
                try
                {
                    actionOnCompletion();
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, e.ToString());
                }

                _isAsync = false;
                Dispose();
            });
        }


        public void PostQuery(Action<DBCommand> actionOnCompletion)
        {
            _isAsync = true;
            SpinWorker.PostWork(() =>
            {
                try
                {
                    Query();
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, CommandText.ToString());
                    Logger.Write(LogType.Err, 1, e.ToString());
                }
            },
            () =>
            {
                try
                {
                    actionOnCompletion(this);
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, e.ToString());
                }

                _isAsync = false;
                Dispose();
            });
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
