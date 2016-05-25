using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra;
using BIMRL;
using BIMRL.OctreeLib;

namespace BIMRLtoCassandra
{
    public class QueryCassDB
    {
        private static ISession _currSession = null;
        
        public QueryCassDB()
        {
            _currSession = CassandraDB.connect();
        }

        public List<BIMRLFedModel> getCassFedModels()
        {
            List<BIMRLFedModel> fedModels = new List<BIMRLFedModel>();
            var prepStmt = _currSession.Prepare("Select projectnumber, projectname, federatedid, lastupdatedate, maxoctreelevel, projectid, worldbbox from bimrl_federatedmodel");
            var stmt = prepStmt.Bind().SetPageSize(100);
            var res = _currSession.Execute(stmt);
            foreach (var row in res)
            {
                BIMRLFedModel mData = new BIMRLFedModel();
                mData.FederatedID = row.GetValue<Int32>(2);
                mData.ProjectNumber = row.GetValue<string>(0);
                mData.ProjectName = row.GetValue<string>(1);
                mData.LastUpdateDate = row.GetValue<DateTime>(3);
                mData.OctreeMaxDepth = row.GetValue<int>(4);
                mData.WorldBoundingBox = row.GetValue<CSGeometry>(5).ToString();

                fedModels.Add(mData);
            }

            return fedModels;
        }
    }
}
