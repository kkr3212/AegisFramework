using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Aegis.Threading;



namespace Aegis.Data.MySQL
{
    [DebuggerDisplay("Name={DBName}, Host={IpAddress},{PortNo}")]
    public sealed class ConnectionPool
    {
        private BlockingQueue<DBConnector> _poolDBC = new BlockingQueue<DBConnector>();
        private List<DBConnector> _dbConnectors = new List<DBConnector>();
        private CancellationTokenSource _cancelTasks;
        private int _maxDBCCount = 4;


        public string DBName { get; private set; }
        public string UserId { get; private set; }
        public string UserPwd { get; private set; }
        public string CharSet { get; private set; }
        public string IpAddress { get; private set; }
        public int PortNo { get; private set; }
        public int PooledDBCCount { get { return _poolDBC.Count; } }
        public int ActiveDBCCount { get { return _dbConnectors.Count - _poolDBC.Count; } }
        public int MaxDBCCount
        {
            get { return _maxDBCCount; }
            set
            {
                if (value > _maxDBCCount)
                    _maxDBCCount = value;
            }
        }





        public ConnectionPool()
        {
        }


        public ConnectionPool(string ipAddress, int portNo, string charSet, string dbName, string userId, string userPwd)
        {
            Initialize(ipAddress, portNo, charSet, dbName, userId, userPwd);
        }


        public void Initialize(string ipAddress, int portNo, string charSet, string dbName, string userId, string userPwd)
        {
            if (_cancelTasks != null)
                throw new AegisException(AegisResult.AlreadyInitialized);


            //  Connection Test
            try
            {
                DBConnector dbc = new DBConnector();
                dbc.Connect(ipAddress, portNo, charSet, dbName, userId, userPwd);
                dbc.Close();
            }
            catch (Exception e)
            {
                throw new AegisException(AegisResult.MySqlConnectionFailed, e, "Invalid MySQL connection.");
            }


            IpAddress = ipAddress;
            PortNo = portNo;
            CharSet = charSet;
            DBName = dbName;
            UserId = userId;
            UserPwd = userPwd;


            _cancelTasks = new CancellationTokenSource();
            PingTest();
        }


        public void Release()
        {
            lock (this)
            {
                _cancelTasks?.Cancel();
                _cancelTasks?.Dispose();
                _cancelTasks = null;


                _dbConnectors.ForEach(v => v.Close());
                _poolDBC.Clear();
                _dbConnectors.Clear();
            }
        }


        private async void PingTest()
        {
            while (_cancelTasks.IsCancellationRequested == false)
            {
                try
                {
                    await Task.Delay(60000, _cancelTasks.Token);


                    //  연결유지를 위해 동작중이 아닌 DBConnector의 Ping을 한번씩 호출한다.
                    int cnt = _poolDBC.Count;
                    while (cnt-- > 0)
                    {
                        DBConnector dbc = GetDBC();
                        dbc.Ping();
                        ReturnDBC(dbc);
                    }
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception e)
                {
                    Logger.Warn(LogMask.Aegis, e.Message);
                }
            }
        }


        public void IncreasePool(int count)
        {
            while (count-- > 0)
            {
                DBConnector dbc = new DBConnector();
                dbc.Connect(IpAddress, PortNo, CharSet, DBName, UserId, UserPwd);


                _poolDBC.Enqueue(dbc);
            }
        }


        public DBCommand NewCommand(int timeoutSec = 60)
        {
            return new DBCommand(this, timeoutSec);
        }


        internal DBConnector GetDBC()
        {
            lock (this)
            {
                if (_poolDBC.Count == 0 &&
                    PooledDBCCount + ActiveDBCCount < _maxDBCCount)
                {
                    var dbc = new DBConnector();
                    dbc.Connect(IpAddress, PortNo, CharSet, DBName, UserId, UserPwd);
                    _poolDBC.Enqueue(dbc);
                }
            }


            return _poolDBC.Dequeue();
        }


        internal void ReturnDBC(DBConnector dbc)
        {
            _poolDBC.Enqueue(dbc);
        }


        public int GetTotalQPS()
        {
            int qps = 0;


            lock (_dbConnectors)
            {
                _dbConnectors.ForEach(v => qps += v.QPS.Value);
            }

            return qps;
        }
    }
}
