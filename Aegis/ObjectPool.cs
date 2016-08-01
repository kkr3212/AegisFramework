using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;



namespace Aegis
{
    public interface IObjectPoolInstance
    {
        void Release();
    }





    public static class ObjectPoolManager
    {
        private static List<IObjectPoolInstance> _list = new List<IObjectPoolInstance>();

        public delegate void PopObjectDelegator(object instance, int pooledCount);
        public delegate void PushObjectDelegator(object instance, int pooledCount);
        public static event PopObjectDelegator OnPopObject;
        public static event PopObjectDelegator OnPushObject;





        internal static void Add(IObjectPoolInstance opi)
        {
            //  ObjectPoolInstance는 인스턴스가 static이기 때문에
            //  생성만 되고 프로그램이 종료되기 전까지는 지워지지 않는다.
            //  그러므로 _list를 Clear시킬 수 없다.
            lock (_list)
                _list.Add(opi);
        }


        internal static void ReleaseAll()
        {
            lock (_list)
            {
                foreach (IObjectPoolInstance opm in _list)
                    opm.Release();
            }
        }


        internal static void CallPopObject(object instance, int pooledCount)
        {
            OnPopObject?.Invoke(instance, pooledCount);
        }


        internal static void CallPushObject(object instance, int pooledCount)
        {
            OnPushObject?.Invoke(instance, pooledCount);
        }
    }





    public sealed class ObjectPool<T> : IObjectPoolInstance
    {
        private static ObjectPool<T> _instance = new ObjectPool<T>();
        private Queue<T> _queue = new Queue<T>();
        private Func<T> _generator;





        private ObjectPool()
        {
            ObjectPoolManager.Add(this);
        }


        public static void Allocate(Func<T> generator, int initPoolSize)
        {
            lock (_instance)
            {
                _instance._generator = generator;

                for (int i = 0; i < initPoolSize; ++i)
                    _instance._queue.Enqueue(_instance._generator());
            }
        }


        public static void Clear()
        {
            _instance.Release();
        }


        public void Release()
        {
            lock (_instance)
                _instance._queue.Clear();
        }


        public static T Pop(Func<T> generator = null)
        {
            T obj;


            lock (_instance)
            {
                if (generator == null)
                    generator = _instance._generator;


                if (_instance._queue.Count > 0)
                    obj = _instance._queue.Dequeue();
                else
                {
                    if (generator == null)
                    {
                        ConstructorInfo constructor = typeof(T).GetConstructor(
                                                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                                                    , null, new Type[0], null);
                        if (constructor != null)
                            obj = (T)constructor.Invoke(null);
                        else
                            throw new AegisException("No matches constructor on {0}.", typeof(T).Name);
                    }
                    else
                        obj = generator();
                }


                ObjectPoolManager.CallPopObject(obj, _instance._queue.Count());
            }


            return obj;
        }


        public static void Push(T obj)
        {
            lock (_instance)
            {
                _instance._queue.Enqueue(obj);
                ObjectPoolManager.CallPushObject(obj, _instance._queue.Count());
            }
        }


        public static int PooledCount()
        {
            lock (_instance)
            {
                return (int)_instance._queue.Count();
            }
        }
    }





    /*public class PooledObject<T>
    {
        protected PooledObject()
        {
        }


        public static T NewObject()
        {
            return ObjectPool<T>.Pop();
        }


        public static void ReturnObject(T obj)
        {
            ObjectPool<T>.Push(obj);
        }
    }*/
}
