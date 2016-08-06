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
        public readonly int Key;
        public TargetMethodAttribute(int key)
        {
            Key = key;
        }
    }





    public delegate void MethodSelectHandler(ref object source, out int key);

    public class MethodSelector
    {
        private object _target;
        private Dictionary<int, MethodInfo> _methods = new Dictionary<int, MethodInfo>();
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
                        int packetId = (attr as TargetMethodAttribute).Key;
                        MethodInfo tmp;

                        if (_methods.TryGetValue(packetId, out tmp) == true)
                        {
                            Logger.Err(LogMask.Aegis, "DispatchAttribute({0}) already defined on {1}.{2}().",
                                packetId, _target.GetType().Name, tmp.Name);
                            break;
                        }

                        _methods.Add(packetId, methodInfo);
                    }
                }
            }
        }


        public bool Dispatch(object source)
        {
            int key;
            _handler(ref source, out key);


            MethodInfo method;
            if (_methods.TryGetValue(key, out method) == false)
                return false;

            method.Invoke(_target, new[] { source });
            return true;
        }
    }
}
