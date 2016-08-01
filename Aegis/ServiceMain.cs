using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;



namespace Aegis
{
    internal class ServiceMain : ServiceBase
    {
        public static readonly ServiceMain Instance = new ServiceMain();
        public Action EventStart, EventStop;





        private ServiceMain()
        {
        }


        override protected void OnStart(string[] args)
        {
            ServiceStart();
        }


        override protected void OnStop()
        {
            ServiceStop();
        }


        public void ServiceStart()
        {
            EventStart();
        }


        public void ServiceStop()
        {
            EventStop();
        }


        public void Run()
        {
            ServiceBase.Run(this);
        }
    }
}
