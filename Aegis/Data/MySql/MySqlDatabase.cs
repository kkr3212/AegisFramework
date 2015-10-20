using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Aegis.Threading;
using Aegis.Network;



namespace Aegis.Data.MySql
{
    [DebuggerDisplay("Name={DBName}, Host={IpAddress},{PortNo}")]
    public sealed class MySqlDatabase
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
        public Boolean UseConnectionPool { get; set; }
        public Int32 PooledDBCCount { get { return _listPoolDBC.Count; } }
        public Int32 ActiveDBCCount { get { return _listActiveDBC.Count; } }

        internal WorkerThread QueryWorker { get; set; }





        public MySqlDatabase()
        {
        }


        public MySqlDatabase(String ipAddress, Int32 portNo, String charSet, String dbName, String userId, String userPwd)
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
            catch (Exception)
            {
                throw new AegisException(AegisResult.MySqlConnectionFailed, "Invalid MySQL connection.");
            }


            IpAddress = ipAddress;
            PortNo = portNo;
            CharSet = charSet;
            DBName = dbName;
            UserId = userId;
            UserPwd = userPwd;
            UseConnectionPool = true;


            QueryWorker = new WorkerThread(String.Format("DBWorker({0})", dbName));
            _cancelTasks = new CancellationTokenSource();
            PingTest();
        }


        public void Release()
        {
            QueryWorker?.Stop();
            QueryWorker = null;


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


        /// <summary>
        /// 비동기 쿼리를 수행할 Thread의 개수를 설정합니다.
        /// </summary>
        /// <param name="threadCount">비동기 쿼리를 수행할 Thread의 개수</param>
        public void SetThreadCount(Int32 threadCount)
        {
            QueryWorker.Stop();
            QueryWorker.Start(threadCount);
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
                    Logger.Write(LogType.Warn, 0, e.Message);
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
            if (UseConnectionPool == true)
            {
                using (_lock.WriterLock)
                {
                    _listActiveDBC.Remove(dbc);
                    _listPoolDBC.Add(dbc);
                }
            }
            else
                dbc.Close();
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
