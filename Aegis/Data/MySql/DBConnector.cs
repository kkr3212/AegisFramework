using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;



namespace Aegis.Data.MySql
{
    internal sealed class DBConnector : IDisposable
    {
        private MySqlConnection _dbc;
        private IntervalCounter _qps = new IntervalCounter(1000);


        public MySqlDatabase MySql { get; set; }
        public MySqlConnection Connection { get { return _dbc; } }
        public String ConnectionString { get { return _dbc.ConnectionString; } }
        public Int32 QPS { get { return _qps.Value; } }





        internal DBConnector(MySqlDatabase parent)
        {
            MySql = parent;
        }


        public void Dispose()
        {
            MySql.ReturnDBC(this);
        }


        public void Connect(String hostIp, Int32 hostPortNo, String charSet, String dbName, String user, String pwd, Int32 commandTimeoutSec = 60)
        {
            if (_dbc != null)
                return;


            _dbc = new MySqlConnection(String.Format("Server={0};Port={1};CharSet={2};Database={3};Uid={4};Pwd={5};"
                                            , hostIp, hostPortNo, charSet, dbName, user, pwd));
            _dbc.Open();
            _qps.Reset();


            using (DBCommand cmd = DBCommand.NewCommand(this))
            {
                cmd.Query("set transaction isolation level read uncommitted;");
                cmd.Query("set session wait_timeout=604800;set session interactive_timeout=604800;");
            }
        }


        public void Close()
        {
            if (_dbc != null)
            {
                _dbc.Close();
                _dbc = null;
            }
        }


        public void Ping()
        {
            if (_dbc != null)
                _dbc.Ping();
        }


        internal void IncreaseQueryCount()
        {
            _qps.Add(1);
        }
    }
}
