using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



namespace Aegis
{
    /// <summary>
    /// Queue에서 요소를 가져올 때 Queue가 비어있다면 새로운 요소가 삽입될 때 까지 대기합니다.
    /// </summary>
    /// <typeparam name="T">Queue의 요소 형식을 지정합니다.</typeparam>
    public class BlockingQueue<T>
    {
        private Queue<T> _queue = new Queue<T>();
        private int _queuedCount = 0;
        private bool _canceled = false;

        public List<T> Items { get { return _queue.ToList(); } }
        public int Count { get { return _queuedCount; } }





        public void ForEach(Action<T> action)
        {
            lock (_queue)
            {
                foreach (T item in _queue)
                    action(item);
            }
        }


        /// <summary>
        /// Queue에 객체 하나를 추가하고 대기중인(Pop을 호출한) 쓰레드 하나가 깨어납니다.
        /// </summary>
        /// <param name="item">추가할 객체</param>
        public void Enqueue(T item)
        {
            lock (_queue)
            {
                _queue.Enqueue(item);
                _queuedCount = _queue.Count;
                //Monitor.PulseAll(_queue);
                Monitor.Pulse(_queue);
            }
        }


        /// <summary>
        /// Queue에서 객체 하나를 가져옵니다. 만약 Queue가 비어있는 상태라면 객체가 추가될 때 까지 대기합니다.
        /// 반환되는 객체는 Queue에서 제거됩니다.
        /// </summary>
        /// <returns>Queue의 첫 번째에 위치한 객체</returns>
        public T Dequeue()
        {
            lock (_queue)
            {
                while (_queue.Count == 0 && _canceled == false)
                    Monitor.Wait(_queue);

                if (_canceled == true)
                    throw new JobCanceledException("Pop call stopped by requested cancellation in BlockingQueue<{0}>.", typeof(T).ToString());


                T item = _queue.Dequeue();
                _queuedCount = _queue.Count;

                return item;
            }
        }


        /// <summary>
        /// Queue에서 객체 하나를 가져옵니다. 만약 Queue가 비어있는 상태라면 default(T)를 반환합니다.
        /// 반환되는 객체는 Queue에서 제거됩니다.
        /// </summary>
        /// <returns>Queue의 첫 번째에 위치한 객체 혹은 default 값</returns>
        public T DequeueOrDefault()
        {
            lock (_queue)
            {
                if (_queue.Count == 0)
                    return default(T);


                T item = _queue.Dequeue();
                _queuedCount = _queue.Count;
                return item;
            }
        }


        public void Clear()
        {
            lock (_queue)
            {
                _queue.Clear();
                _queuedCount = 0;
                _canceled = false;

                Monitor.PulseAll(_queue);
            }
        }


        /// <summary>
        /// Dequeue에 대기중인 쓰레드에서 JobCanceledException이 발생합니다.
        /// Clear를 호출해 초기화하기 전 까지는 Pop을 사용할 수 없습니다.
        /// </summary>
        public void Cancel()
        {
            _canceled = true;

            lock (_queue)
                Monitor.PulseAll(_queue);
        }
    }
}
