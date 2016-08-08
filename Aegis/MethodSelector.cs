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
    public class TargetMethodAttribute : Attribute
    {
        internal readonly string Key;


        public TargetMethodAttribute(string key)
        {
            Key = key;
        }


        public TargetMethodAttribute(ulong key)
        {
            Key = key.ToString();
        }


        public TargetMethodAttribute(long key)
        {
            Key = key.ToString();
        }


        public TargetMethodAttribute(uint key)
        {
            Key = key.ToString();
        }


        public TargetMethodAttribute(int key)
        {
            Key = key.ToString();
        }


        public TargetMethodAttribute(double key)
        {
            Key = key.ToString();
        }
    }





    public class MethodSelector<T>
    {
        public delegate void MethodSelectHandler(ref T source, out string key);

        private object _target;
        private Dictionary<string, MethodInfo> _methods = new Dictionary<string, MethodInfo>();
        private MethodSelectHandler _handler;





        public MethodSelector(object targetInstance, MethodSelectHandler handler)
        {
            _target = targetInstance;
            _handler = handler;


            foreach (var methodInfo in _target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach (var attr in methodInfo.GetCustomAttributes())
                {
                    if (attr is TargetMethodAttribute)
                    {
                        string key = (attr as TargetMethodAttribute).Key;
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

            method.Invoke(_target, new object [] { source });
            return true;
        }
    }
}
