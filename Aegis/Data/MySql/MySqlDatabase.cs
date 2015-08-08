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
        public Int32 ShardKeyStart { get; private set; }
        public Int32 ShardKeyEnd { get; private set; }
        public Boolean UseConnectionPool { get; set; }
        internal WorkerThread WorkerQueue { get; set; }





        public void Initialize(String ipAddress, Int32 portNo, String charSet, String dbName, String userId, String userPwd
            , Int32 shardKeyStart, Int32 shardKeyEnd)
        {
            using (_lock.WriterLock)
            {
                //  초기화
                IpAddress = ipAddress;
                PortNo = portNo;
                CharSet = charSet;
                DBName = dbName;
                UserId = userId;
                UserPwd = userPwd;
                ShardKeyStart = shardKeyStart;
                ShardKeyEnd = shardKeyEnd;

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
                    _cancelTasks.Cancel();


                foreach (DBConnector dbc in _poolDBC)
                    dbc.Close();

                _poolDBC.Clear();
                _dbcCount = 0;

                WorkerQueue.Stop();
            }
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
                        DBConnector dbc = GetDBC();
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


        public Boolean IsInShardKey(Int32 shardKey)
        {
            if (shardKey >= ShardKeyStart && shardKey <= ShardKeyEnd)
                return true;

            return false;
        }


        internal DBConnector GetDBC()
        {
            DBConnector dbc;


            try
            {
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
            }
            catch (Exception e)
            {
                Logger.Write(LogType.Err, 1, e.Message);
                throw e;
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
