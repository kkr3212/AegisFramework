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


        public String DBName { get; private set; }
        public String UserId { get; private set; }
        public String UserPwd { get; private set; }
        public String CharSet { get; private set; }
        public String IpAddress { get; private set; }
        public Int32 PortNo { get; private set; }
        public Int32 PooledDBCCount { get { return _listPoolDBC.Count; } }
        public Int32 ActiveDBCCount { get { return _listActiveDBC.Count; } }





        public ConnectionPool()
        {
        }


        public ConnectionPool(String ipAddress, Int32 portNo, String charSet, String dbName, String userId, String userPwd)
        {
            Initialize(ipAddress, portNo, charSet, dbName, userId, userPwd);
        }


        public void Initialize(String ipAddress, Int32 portNo, String charSet, String dbName, String userId, String userPwd)
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
                    Int32 cnt = _listPoolDBC.Count;
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
                    Logger.Write(LogType.Warn, 1, e.Message);
                }
            }
        }


        public void IncreasePool(Int32 count)
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


        public Int32 GetTotalQPS()
        {
            Int32 qps = 0;


            using (_lock.ReaderLock)
            {
                _listPoolDBC.ForEach(v => qps += v.QPS.Value);
                _listActiveDBC.ForEach(v => qps += v.QPS.Value);
            }

            return qps;
        }
    }
}
