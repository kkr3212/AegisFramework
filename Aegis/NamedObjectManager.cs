using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis.Threading;



namespace Aegis
{
    public static class NamedObjectManager
    {
        //  이름의 대소문자 구분을 없애기 위해 Comparer를 재정의
        private class NameComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.OrdinalIgnoreCase);
            }


            public int GetHashCode(string x)
            {
                return x.GetHashCode();
            }
        }

        private static Dictionary<string, object> _objects = new Dictionary<string, object>(new NameComparer());
        private static RWLock _lock = new RWLock();





        public static void Clear()
        {
            using (_lock.WriterLock)
            {
                _objects.Clear();
            }
        }


        public static void Add(string name, object obj)
        {
            using (_lock.WriterLock)
            {
                object tmp;
                if (_objects.TryGetValue(name, out tmp) == true)
                    throw new AegisException(AegisResult.AlreadyExistName, "'{0}' is already exists in {1}.", name, nameof(NamedObjectManager));

                _objects.Add(name, obj);
            }
        }


        public static T Add<T>(string name, object obj)
        {
            using (_lock.WriterLock)
            {
                object tmp;
                if (_objects.TryGetValue(name, out tmp) == true)
                    throw new AegisException(AegisResult.AlreadyExistName, "'{0}' is already exists in {1}.", name, nameof(NamedObjectManager));

                _objects.Add(name, obj);

                return (T)obj;
            }
        }


        public static void Remove(string name)
        {
            using (_lock.WriterLock)
            {
                _objects.Remove(name);
            }
        }


        public static object Get(string name)
        {
            using (_lock.ReaderLock)
            {
                object obj;
                if (_objects.TryGetValue(name, out obj) == false)
                    throw new AegisException(AegisResult.NotExistName);

                return obj;
            }
        }


        public static T Get<T>(string name)
        {
            using (_lock.ReaderLock)
            {
                object obj;
                if (_objects.TryGetValue(name, out obj) == false)
                    throw new AegisException(AegisResult.NotExistName);

                return (T)obj;
            }
        }


        public static object Find(string name)
        {
            using (_lock.ReaderLock)
            {
                object obj;
                if (_objects.TryGetValue(name, out obj) == false)
                    return null;

                return obj;
            }
        }


        public static T Find<T>(string name)
        {
            using (_lock.ReaderLock)
            {
                object obj;
                if (_objects.TryGetValue(name, out obj) == false)
                    return default(T);

                return (T)obj;
            }
        }


        public static bool Exists(string name)
        {
            using (_lock.ReaderLock)
            {
                object obj;
                if (_objects.TryGetValue(name, out obj) == true)
                    return true;

                return false;
            }
        }
    }





    public class NamedObjectIndexer<T>
    {
        public T this[string name]
        {
            get { return NamedObjectManager.Find<T>(name); }
        }
        public IEnumerable<T> Values
        {
            get { return Items.Select(v => v.Item2); }
        }
        public readonly List<Tuple<string, T>> Items = new List<Tuple<string, T>>();





        public NamedObjectIndexer()
        {
        }


        public bool Exists(string name)
        {
            T obj = NamedObjectManager.Find<T>(name);
            return Items.Exists(v => v.Equals(obj));
        }


        public void Add(string name, T obj)
        {
            NamedObjectManager.Add<T>(name, obj);
            Items.Add(new Tuple<string, T>(name, obj));
        }


        public void Clear()
        {
            foreach (var item in Items)
                NamedObjectManager.Remove(item.Item1);
            Items.Clear();
        }


        public void Remove(string name)
        {
            var item = Items.Find(v => string.Compare(v.Item1, name, StringComparison.OrdinalIgnoreCase) == 0);
            if (item == null)
                return;

            Items.Remove(item);
            NamedObjectManager.Remove(item.Item1);
        }


        public void Remove(T obj)
        {
            var item = Items.Find(v => v.Item2.Equals(obj));
            if (item == null)
                return;

            Items.Remove(item);
            NamedObjectManager.Remove(item.Item1);
        }
    }
}
