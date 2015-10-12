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
    public sealed class MySqlDatabase : IDisposable
    {
        private Queue<DBConnector> _poolDBC = new Queue<DBConnector>();
        private RWLock _lock = new RWLock();
        private Int32 _dbcCount;
        private CancellationTokenSource _cancelTasks;


        public String DBName { get; private set; }
        public String UserId { get; private set; }
        public String UserPwd { get; private set; }
        public String CharSet { get; private set; }
        public String IpAddress { get; private set; }
        public Int32 PortNo { get; private set; }
        public Boolean UseConnectionPool { get; set; }
        internal WorkerThread WorkerQueue { get; set; }





        public MySqlDatabase()
        {
        }


        public MySqlDatabase(String ipAddress, Int32 portNo, String charSet, String dbName, String userId, String userPwd)
        {
            Initialize(ipAddress, portNo, charSet, dbName, userId, userPwd);
        }


        public void Initialize(String ipAddress, Int32 portNo, String charSet, String dbName, String userId, String userPwd)
        {
            //  Connection Test
            using (DBConnector dbc = new DBConnector(null))
            {
                try
                {
                    dbc.Connect(ipAddress, portNo, charSet, dbName, userId, userPwd);
                    dbc.Close();
                }
                catch (Exception)
                {
                    throw new AegisException(AegisResult.MySqlConnectionFailed, "Invalid MySQL connection.");
                }
            }


            using (_lock.WriterLock)
            {
                IpAddress = ipAddress;
                PortNo = portNo;
                CharSet = charSet;
                DBName = dbName;
                UserId = userId;
                UserPwd = userPwd;

                _dbcCount = 0;
                UseConnectionPool = true;
                WorkerQueue = new WorkerThread(String.Format("DBWorker({0})", dbName));
            }


            _cancelTasks = new CancellationTokenSource();
            PingTest();
        }


        public void Release()
        {
            using (_lock.WriterLock)
            {
                if (_cancelTasks != null)
                {
                    _cancelTasks.Cancel();
                    _cancelTasks.Dispose();
                }


                foreach (DBConnector dbc in _poolDBC)
                    dbc.Close();

                _poolDBC.Clear();
                _dbcCount = 0;

                WorkerQueue.Stop();
            }
        }


        public void Dispose()
        {
            Release();
        }


        public void SetWorketQueue(Int32 threadCount)
        {
            using (_lock.WriterLock)
            {
                WorkerQueue.Stop();
                WorkerQueue.Start(threadCount);
            }
        }


        private async void PingTest()
        {
            while (_cancelTasks.IsCancellationRequested == false)
            {
                try
                {
                    await Task.Delay(60000, _cancelTasks.Token);


                    //  모든 DBConnector의 Ping을 한번씩 호출한다.
                    Int32 cnt = _poolDBC.Count();
                    while (cnt-- > 0)
                    {
                        using (DBConnector dbc = GetDBC())
                            dbc.Ping();
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


        public void GetCount(out Int32 dbcCount, out Int32 activeCount)
        {
            using (_lock.ReaderLock)
            {
                dbcCount = _dbcCount;
                activeCount = _dbcCount - _poolDBC.Count;
            }
        }


        public void IncreasePool(Int32 count)
        {
            while (count-- > 0)
            {
                DBConnector dbc = new DBConnector(this);
                dbc.Connect(IpAddress, PortNo, CharSet, DBName, UserId, UserPwd);
                ++_dbcCount;


                using (_lock.WriterLock)
                {
                    _poolDBC.Enqueue(dbc);
                }
            }
        }


        internal DBConnector GetDBC()
        {
            DBConnector dbc;


            using (_lock.WriterLock)
            {
                if (_poolDBC.Count() == 0)
                {
                    dbc = new DBConnector(this);
                    dbc.Connect(IpAddress, PortNo, CharSet, DBName, UserId, UserPwd);
                    ++_dbcCount;
                }
                else
                    dbc = _poolDBC.Dequeue();
            }

            return dbc;
        }


        internal void ReturnDBC(DBConnector dbc)
        {
            if (UseConnectionPool == true)
            {
                using (_lock.WriterLock)
                {
                    _poolDBC.Enqueue(dbc);
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
                foreach (DBConnector dbc in _poolDBC)
                    qps += dbc.QPS;
            }

            return qps;
        }
    }
}
