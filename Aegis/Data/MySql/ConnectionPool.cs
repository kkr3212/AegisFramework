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
    [DebuggerDisplay("Name={DBName}, Host={IpAddress}, {PortNo}")]
    public sealed class ConnectionPool
    {
        private List<DBConnector> _listPoolDBC = new List<DBConnector>();
        private List<DBConnector> _listActiveDBC = new List<DBConnector>();
        private RWLock _lock = new RWLock();
        private CancellationTokenSource _cancelTasks;


        public string DBName { get; private set; }
        public string UserId { get; private set; }
        public string UserPwd { get; private set; }
        public string CharSet { get; private set; }
        public string IpAddress { get; private set; }
        public int PortNo { get; private set; }
        public int PooledDBCCount { get { return _listPoolDBC.Count; } }
        public int ActiveDBCCount { get { return _listActiveDBC.Count; } }





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


        private async void PingTest()
        {
            while (_cancelTasks.IsCancellationRequested == false)
            {
                try
                {
                    await Task.Delay(60000, _cancelTasks.Token);


                    //  연결유지를 위해 동작중이 아닌 DBConnector의 Ping을 한번씩 호출한다.
                    int cnt = _listPoolDBC.Count;
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
                    Logger.Write(LogType.Warn, LogLevel.Core, e.Message);
                }
            }
        }


        public void IncreasePool(int count)
        {
            while (count-- > 0)
            {
                DBConnector dbc = new DBConnector();
                dbc.Connect(IpAddress, PortNo, CharSet, DBName, UserId, UserPwd);


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
                    dbc.Connect(IpAddress, PortNo, CharSet, DBName, UserId, UserPwd);
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
