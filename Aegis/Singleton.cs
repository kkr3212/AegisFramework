using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;



namespace Aegis
{
    public static class Singleton<T> where T : class
    {
        private static volatile T _instance;
        private static object _lock = new object();



        static Singleton()
        {
        }


        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            ConstructorInfo constructor = typeof(T).GetConstructor(
                                BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);

                            if (constructor == null || constructor.IsAssembly)
                                throw new AegisException("A private or protected constructor is missing about '{0}'.", typeof(T).Name);

                            _instance = (T)constructor.Invoke(null);
                        }
                    }
                }

                return _instance;
            }
        }
    }
}
