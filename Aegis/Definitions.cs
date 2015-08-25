using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;



namespace Aegis
{
    public static class Version
    {
        public static readonly Int32 Major = Assembly.GetExecutingAssembly().GetName().Version.Major;
        public static readonly Int32 Minor = Assembly.GetExecutingAssembly().GetName().Version.Minor;
        public static readonly Int32 Build = Assembly.GetExecutingAssembly().GetName().Version.Build;
        public static readonly Int32 Revision = Assembly.GetExecutingAssembly().GetName().Version.Revision;

        public static new String ToString()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }



    public delegate Network.SessionBase SessionGenerateDelegator();
}
