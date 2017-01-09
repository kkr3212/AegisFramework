using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Aegis.IO;



namespace Aegis
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DispatchMethodAttribute : Attribute
    {
        internal readonly string Key;


        public DispatchMethodAttribute(string key)
        {
            Key = key;
        }


        public DispatchMethodAttribute(ulong key)
        {
            Key = key.ToString();
        }


        public DispatchMethodAttribute(long key)
        {
            Key = key.ToString();
        }


        public DispatchMethodAttribute(uint key)
        {
            Key = key.ToString();
        }


        public DispatchMethodAttribute(int key)
        {
            Key = key.ToString();
        }


        public DispatchMethodAttribute(double key)
        {
            Key = key.ToString();
        }
    }





    public class DispatchMethodSelector<T>
    {
        public delegate void MethodSelectHandler(ref T source, out string key);

        private object _target;
        private Dictionary<string, MethodInfo> _methods = new Dictionary<string, MethodInfo>();
        private MethodSelectHandler _handler;





        public DispatchMethodSelector(object targetInstance, MethodSelectHandler handler)
        {
            if (targetInstance == null)
                throw new AegisException(AegisResult.InvalidArgument, "{0} cannot be null.", nameof(targetInstance));
            if (handler == null)
                throw new AegisException(AegisResult.InvalidArgument, "{0} cannot be null.", nameof(handler));

            _target = targetInstance;
            _handler = handler;


            foreach (var methodInfo in _target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach (var attr in methodInfo.GetCustomAttributes())
                {
                    if (attr is DispatchMethodAttribute)
                    {
                        string key = (attr as DispatchMethodAttribute).Key;
                        MethodInfo tmp;

                        if (_methods.TryGetValue(key, out tmp) == true)
                        {
                            Logger.Err(LogMask.Aegis, "MethodSelector key(={0}) duplicated defined.", key);
                            break;
                        }

                        _methods.Add(key, methodInfo);
                    }
                }
            }
        }


        public bool Dispatch(T source)
        {
            string key;
            _handler(ref source, out key);


            MethodInfo method;
            if (_methods.TryGetValue(key, out method) == false)
                return false;

            method.Invoke(_target, new object[] { source });
            return true;
        }


        public bool HasMethodWithSource(T source)
        {
            string key;
            _handler(ref source, out key);


            MethodInfo method;
            return _methods.TryGetValue(key, out method);
        }


        public bool HasMethodWithKey(string key)
        {
            MethodInfo method;
            return _methods.TryGetValue(key, out method);
        }
    }





    public class DispatchMethodSelector<T, TResult>
    {
        public delegate void MethodSelectHandler(ref T source, out string key);

        private object _target;
        private Dictionary<string, MethodInfo> _methods = new Dictionary<string, MethodInfo>();
        private MethodSelectHandler _handler;





        public DispatchMethodSelector(object targetInstance, MethodSelectHandler handler)
        {
            if (targetInstance == null)
                throw new AegisException(AegisResult.InvalidArgument, "{0} cannot be null.", nameof(targetInstance));
            if (handler == null)
                throw new AegisException(AegisResult.InvalidArgument, "{0} cannot be null.", nameof(handler));

            _target = targetInstance;
            _handler = handler;


            foreach (var methodInfo in _target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach (var attr in methodInfo.GetCustomAttributes())
                {
                    if (attr is DispatchMethodAttribute)
                    {
                        string key = (attr as DispatchMethodAttribute).Key;
                        MethodInfo tmp;

                        if (_methods.TryGetValue(key, out tmp) == true)
                        {
                            Logger.Err(LogMask.Aegis, "MethodSelector key(={0}) duplicated defined.", key);
                            break;
                        }
                        if (methodInfo.ReturnType != typeof(TResult))
                        {
                            Logger.Err(LogMask.Aegis, "Invalid return type declared in {0}.{1}", _target.ToString(), methodInfo.Name);
                            break;
                        }

                        _methods.Add(key, methodInfo);
                    }
                }
            }
        }


        public bool Dispatch(T source, out TResult result)
        {
            string key;
            _handler(ref source, out key);


            MethodInfo method;
            if (_methods.TryGetValue(key, out method) == false)
            {
                result = default(TResult);
                return false;
            }

            result = (TResult)method.Invoke(_target, new object [] { source });
            return true;
        }


        public bool HasMethodWithSource(T source)
        {
            string key;
            _handler(ref source, out key);


            MethodInfo method;
            return _methods.TryGetValue(key, out method);
        }


        public bool HasMethodWithKey(string key)
        {
            MethodInfo method;
            return _methods.TryGetValue(key, out method);
        }
    }
}
