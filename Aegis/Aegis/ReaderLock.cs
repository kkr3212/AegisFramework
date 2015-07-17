using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



namespace Aegis
{
    public sealed class ReaderLock : IDisposable
    {
        private ReaderWriterLockSlim _lock;


        public ReaderLock(ReaderWriterLockSlim lockObj)
        {
            _lock = lockObj;
        }


        public ReaderLock Enter()
        {
            _lock.EnterReadLock();
            return this;
        }


        public void Dispose()
        {
            _lock.ExitReadLock();
        }
    }



    public sealed class WriterLock : IDisposable
    {
        private ReaderWriterLockSlim _lock;


        public WriterLock(ReaderWriterLockSlim lockObj)
        {
            _lock = lockObj;
        }


        public WriterLock Enter()
        {
            _lock.EnterWriteLock();
            return this;
        }


        public void Dispose()
        {
            _lock.ExitWriteLock();
        }
    }



    public sealed class RWLock
    {
        private ReaderWriterLockSlim _lock;
        private ReaderLock _lockRead;
        private WriterLock _lockWrite;


        public ReaderLock ReaderLock { get { return _lockRead.Enter(); } }
        public WriterLock WriterLock { get { return _lockWrite.Enter(); } }


        public RWLock()
        {
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _lockRead = new ReaderLock(_lock);
            _lockWrite = new WriterLock(_lock);
        }
    }
}
