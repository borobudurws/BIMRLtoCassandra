using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Data;


namespace BIMRLtoCassandra
{
    public static class CassandraDB
    {
        //static private CqlConnection m_CSConn = null;
        static public Cluster m_CSConn = null;
        static public ISession m_CSSession = null;

        //public static CqlConnection connect()
        //{
        //    return connect(null);
        //}

        //public static CqlConnection connect(string connectStr)
        //{
            //if (m_CSConn != null)
            //    return m_CSConn;
            //else
            //{
            //    try
            //    {
            //        m_CSConn = new CqlConnection(connectStr);
            //        m_CSConn.Open();
            //        m_CSConn.ChangeDatabase("bimassure");
            //        return m_CSConn;
            //    }
            //    catch (Exception e)
            //    {
            //        string errorMsg = e.Message;
            //        throw;
            //    }
            //}
        //}

        public static ISession connect()
        {
            return connect("localhost");
        }

        public static ISession connect(string node)
        {
            if (m_CSConn == null)
            {
                m_CSConn = Cluster.Builder().AddContactPoint(node).Build();
            }

            if (m_CSSession == null)
            {
                m_CSSession = m_CSConn.Connect("bimassure");
            }

            return m_CSSession;
        }

        public static void disconnect()
        {
            if (m_CSConn != null)
            {
                m_CSSession.Dispose();
                m_CSSession = null;
                m_CSConn.Dispose();
                m_CSConn = null;
            }
        }

    }
}
