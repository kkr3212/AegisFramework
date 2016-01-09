using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;



namespace Aegis.Data.MSSQL
{
    internal sealed class DBConnector
    {
        public SqlConnection Connection { get; private set; }
        public String ConnectionString { get { return Connection.ConnectionString; } }
        public IntervalCounter QPS { get; private set; }





        internal DBConnector()
        {
        }


        public void Connect(String host, String userId, String userPwd, String dbName)
        {
            if (Connection != null)
                return;


            QPS = new IntervalCounter(1000);
            Connection = new SqlConnection(String.Format("server={0};uid={1};pwd={2};database={3};", host, userId, userPwd, dbName));
            Connection.Open();
        }


        public void Close()
        {
            if (Connection != null)
            {
                Connection.Close();
                Connection = null;
            }
        }
    }
}
