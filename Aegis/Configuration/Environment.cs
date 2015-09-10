using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;



namespace Aegis.Configuration
{
    public static class Environment
    {
        public static Assembly ExecutingAssembly { get; internal set; }
        public static Version AegisVersion { get; private set; }
        public static Version ExecutingVersion { get; private set; }





        static Environment()
        {
            ExecutingAssembly = Assembly.GetEntryAssembly();

            AegisVersion = new Version(
                Assembly.GetExecutingAssembly().GetName().Version.Major,
                Assembly.GetExecutingAssembly().GetName().Version.Minor,
                Assembly.GetExecutingAssembly().GetName().Version.Build,
                Assembly.GetExecutingAssembly().GetName().Version.Revision);

            ExecutingVersion = new Version(
                ExecutingAssembly.GetName().Version.Major,
                ExecutingAssembly.GetName().Version.Minor,
                ExecutingAssembly.GetName().Version.Build,
                ExecutingAssembly.GetName().Version.Revision);
        }
    }
}
