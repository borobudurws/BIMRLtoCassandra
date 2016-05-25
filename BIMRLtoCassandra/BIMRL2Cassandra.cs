using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using BIMRL.OctreeLib;
using Cassandra;
using Cassandra.Data;

namespace BIMRLtoCassandra
{
    public class BIMRL2Cassandra
    {
        HashSet<string> elementProcessed = new HashSet<string>();

        public BIMRL2Cassandra()
        {
        }
        
        public void copyData(int fedID, string projectNumber, string projectName)
        {
            OracleConnection oraConn = DBOperation.Connect();
            ISession CSSession = CassandraDB.connect();
            CSSession.UserDefinedTypes.Define(UdtMap.For<CSType>(), UdtMap.For<CSPoint>(), UdtMap.For<CSGeometry>(), UdtMap.For<CSMatrix3D>(), UdtMap.For<CSSpatialIndex>(),
                                            UdtMap.For<CSTopoFace>(), UdtMap.For<CSProperty>(), UdtMap.For<CSMaterial>(), UdtMap.For<CSClassification>());

            PreparedStatement modelIns = CSSession.Prepare("Insert into bimrl_federatedmodel (projectnumber, projectname, federatedid, lastupdatedate, maxoctreelevel, projectid, worldbox) "
                                            + "values (?, ?, ?, ?, ?, ?, ?)");
            PreparedStatement elemInsPrep = CSSession.Prepare("Insert into bimrl_element (federatedmodelid, projectname, projectnumber, elementid, lineno, elementtype, modelid, type, name, longname, "
                                            + "ownerhistoryid, description, objecttype, tag, container, geometrybody, geometrybody_bbox, geometrybody_bbox_centroid, geometryfootprint, geometryaxis, "
                                            + "transform, obb_major_axis, obb_major_axis_centroid, obb) "
                                            + "values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?,  ?, ?, ?, ?, ?, ?, ?, ?, ?, ?,  ?, ?, ?, ?)");

            // Delete existing data first if already exists
            deleteData(fedID);

            string sqlStmt = string.Empty;

            sqlStmt = "Select e.elementid, e.lineno, e.elementtype, e.modelid, e.typeid, e.name, e.longname, e.ownerhistoryid, e.description, e.objecttype, "
                            + "e.tag, e.container, e.geometrybody, e.geometrybody_bbox, e.geometrybody_bbox_centroid, e.geometryfootprint, e.geometryaxis, e.transform_x_axis, e.transform_y_axis, e.transform_z_axis, "
                            + "e.body_major_axis1, e.body_major_axis2, e.body_major_axis3, e.body_major_axis_cnetroid, e.obb, t.elementid, t.ifctype, t.name, t.description, t.ownerhistoryid, "
                            + "t.modelid, t.applicableoccurence, t.tag, t.elementtype, t.predefinedtype, t.assembyplace, t.operationtype, t.constructiontype "
                            + "from bimrl_element_" + fedID.ToString("X4") + " e, bimrl_type_" + fedID.ToString("X4") + " t where t.elementid (+) =e.typeid";

            OracleCommand command = new OracleCommand(sqlStmt, oraConn);
            command.CommandText = sqlStmt;
            command.FetchSize = 1000;
            OracleDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                CSType bType = new CSType();
                bType.typeid = reader.GetString(22);
                bType.ifctype = reader.GetString(23);
                bType.name = reader.GetString(24);
                bType.description = reader.GetString(25);
                bType.ownerhistoryid = reader.GetInt32(26);
                bType.applicableoccurence = SafeGetString(reader, 27);
                bType.tag = SafeGetString(reader, 28);
                bType.elementtype = reader.GetString(29);
                bType.predefinedtype = reader.GetString(30);
                bType.assemblyplace = reader.GetString(31);
                bType.operationtype = reader.GetString(32);
                bType.constructiontype = reader.GetString(33);

                List<CSPoint>  transf = new List<CSPoint>();
                transf.Add(CSpointFromSdoGeom(reader, 17));
                transf.Add(CSpointFromSdoGeom(reader, 18));
                transf.Add(CSpointFromSdoGeom(reader, 19));
                CSMatrix3D transform = new CSMatrix3D();
                transform.matrix3d = transf;

                List<CSPoint>  mjAxis = new List<CSPoint>();
                mjAxis.Add(CSpointFromSdoGeom(reader, 20));
                mjAxis.Add(CSpointFromSdoGeom(reader, 21));
                mjAxis.Add(CSpointFromSdoGeom(reader, 22));
                CSMatrix3D mjAxisMatrix = new CSMatrix3D();
                mjAxisMatrix.matrix3d = mjAxis;
                
                NetSdoGeometry.SdoGeometry bodyMjAxisCentroid = reader.GetValue(20) as NetSdoGeometry.SdoGeometry;
                NetSdoGeometry.SdoGeometry obb = reader.GetValue(21) as NetSdoGeometry.SdoGeometry;

                BoundStatement boundStmt = elemInsPrep.Bind(fedID, projectName, projectNumber, reader.GetString(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3), bType, SafeGetString(reader, 5), SafeGetString(reader, 6), 
                    SafeGetValue(reader, 7), SafeGetString(reader, 8), SafeGetString(reader, 9), SafeGetString(reader, 10), SafeGetString(reader, 11), CSgeomFromSdoGeom(reader, 12), CSgeomFromSdoGeom(reader, 13), CSpointFromSdoGeom(reader, 14), CSgeomFromSdoGeom(reader, 15), CSgeomFromSdoGeom(reader, 16),
                    transform, mjAxisMatrix, CSpointFromSdoGeom(reader, 23), CSgeomFromSdoGeom(reader, 24));

                CSSession.Execute(boundStmt);
            }
        }

        public bool simple_insert()
        {
            return true;
        }

        public bool async_insert()
        {
            return true;
        }

        public bool batch_insert()
        {
            return true;
        }

        public void deleteData(int FedID)
        {
            ISession CSSession = CassandraDB.connect();
            string cqlStmt = "Delete from bimrl_element where federatedid = " + FedID.ToString();
            CSSession.Execute(cqlStmt);
            cqlStmt = "Delete from bimrl_federatedmodel where federatedid = " + FedID.ToString();
            CSSession.Execute(cqlStmt);
        }

        public static string SafeGetString(OracleDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            else
                return string.Empty;
        }

        public static object SafeGetValue(OracleDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetValue(colIndex);
            else
                return null;
        }

        public static CSGeometry CSgeomFromSdoGeom(OracleDataReader reader, int colIndex)
        {
            CSGeometry CSgeom = null;
            if (!reader.IsDBNull(colIndex))
            {
                NetSdoGeometry.SdoGeometry geomBody = reader.GetValue(colIndex) as NetSdoGeometry.SdoGeometry;
                CSgeom = new CSGeometry();
                CSgeom.geomtype = (short)geomBody.SdoGtype.Value;
                CSgeom.elem_info = geomBody.ElemArrayOfInts.ToList();
                CSgeom.coordinates = geomBody.OrdinatesArrayOfDoubles.ToList();
            }

            return CSgeom;
        }

        public static CSPoint CSpointFromSdoGeom(OracleDataReader reader, int colIndex)
        {
            CSPoint CSpnt = null;
            if (!reader.IsDBNull(colIndex))
            {
                NetSdoGeometry.SdoGeometry geomBody = reader.GetValue(colIndex) as NetSdoGeometry.SdoGeometry;
                CSpnt = new CSPoint();
                CSpnt.x = geomBody.SdoPoint.XD.Value;
                CSpnt.y = geomBody.SdoPoint.YD.Value;
                CSpnt.z = geomBody.SdoPoint.ZD.Value;
            }

            return CSpnt;
        }
    }
}
