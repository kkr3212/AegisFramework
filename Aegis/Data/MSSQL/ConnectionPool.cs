using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Aegis.Threading;



namespace Aegis.Data.MSSQL
{
    [DebuggerDisplay("Name={DBName}, Host={IpAddress},{PortNo}")]
    public sealed class ConnectionPool
    {
        private List<DBConnector> _listPoolDBC = new List<DBConnector>();
        private List<DBConnector> _listActiveDBC = new List<DBConnector>();
        private RWLock _lock = new RWLock();
        private CancellationTokenSource _cancelTasks;


        public string HostAddress { get; private set; }
        public string UserId { get; private set; }
        public string UserPwd { get; private set; }
        public string DBName { get; private set; }
        public int PooledDBCCount { get { return _listPoolDBC.Count; } }
        public int ActiveDBCCount { get { return _listActiveDBC.Count; } }





        public ConnectionPool()
        {
        }


        public ConnectionPool(string host, string userId, string userPwd, string dbName)
        {
            Initialize(host, userId, userPwd, dbName);
        }


        public void Initialize(string host, string userId, string userPwd, string dbName)
        {
            if (_cancelTasks != null)
                throw new AegisException(AegisResult.AlreadyInitialized);


            //  Connection Test
            try
            {
                DBConnector dbc = new DBConnector();
                dbc.Connect(host, userId, userPwd, dbName);
                dbc.Close();
            }
            catch (Exception e)
            {
                throw new AegisException(AegisResult.MySqlConnectionFailed, e, "Invalid MySQL connection.");
            }


            HostAddress = host;
            UserId = userId;
            UserPwd = userPwd;
            DBName = dbName;


            _cancelTasks = new CancellationTokenSource();
        }


        public void Release()
        {
            using (_lock.WriterLock)
            {
                _cancelTasks?.Cancel();
                _cancelTasks?.Dispose();
                _cancelTasks = null;


                _listPoolDBC.ForEach(v => v.Close());
                _listActiveDBC.ForEach(v => v.Close());

                _listPoolDBC.Clear();
                _listActiveDBC.Clear();
            }
        }


        public void IncreasePool(int count)
        {
            while (count-- > 0)
            {
                DBConnector dbc = new DBConnector();
                dbc.Connect(HostAddress, UserId, UserPwd, DBName);


                using (_lock.WriterLock)
                {
                    _listPoolDBC.Add(dbc);
                }
            }
        }


        internal DBConnector GetDBC()
        {
            DBConnector dbc;


            using (_lock.WriterLock)
            {
                if (_listPoolDBC.Count == 0)
                {
                    dbc = new DBConnector();
                    dbc.Connect(HostAddress, UserId, UserPwd, DBName);
                }
                else
                {
                    dbc = _listPoolDBC.ElementAt(0);
                    _listPoolDBC.RemoveAt(0);
                    _listActiveDBC.Add(dbc);
                }
            }

            return dbc;
        }


        internal void ReturnDBC(DBConnector dbc)
        {
            using (_lock.WriterLock)
            {
                _listActiveDBC.Remove(dbc);
                _listPoolDBC.Add(dbc);
            }
        }


        public int GetTotalQPS()
        {
            int qps = 0;


            using (_lock.ReaderLock)
            {
                _listPoolDBC.ForEach(v => qps += v.QPS.Value);
                _listActiveDBC.ForEach(v => qps += v.QPS.Value);
            }

            return qps;
        }
    }
}
