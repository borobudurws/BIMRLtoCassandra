using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIMRL.OctreeLib;

namespace BIMRLtoCassandra
{
    public class CSPoint
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
    }

    public class CSGeometry
    {
        public short geomtype { get; set; }
        public List<int> elem_info { get; set; }
        public List<double> coordinates { get; set; }
    }

    public class CSMatrix3D
    {
        public List<CSPoint> matrix3d { get; set; }
    }

    public class CSTopoFace
    {
        public string id { get; set; }
        public string type { get; set; }
        public CSGeometry polygon { get; set; }
        public CSPoint normal { get; set; }
        public double anglefromnorth { get; set; }
        public CSPoint centroid { get; set; }
        public string orientation { get; set; }
        public string attribute { get; set; }
    }

    public class CSSpatialIndex
    {
        public string cellid { get; set; }
        public int xminbound { get; set; }
        public int yminbound { get; set; }
        public int zminbound { get; set; }
        public int xmaxbound { get; set; }
        public int ymaxbound { get; set; }
        public int zmaxbound { get; set; }
        public short depth { get; set; }
        public short celltype { get; set; }
    }

    public class CSProperty
    {
        public string propertygroupname { get; set; }
        public string propertyname { get; set; }
        public string propertyvalue { get; set; }
        public string propertydatatype { get; set; }
        public string propertyunit { get; set; }
        public bool fromtype { get; set; }
    }

    public class CSMaterial
    {
        public string materialname { get; set; }
        public string category { get; set; }
        public string setname { get; set; }
        public short? materialsequence { get; set; }
        public double? materialthickness { get; set; }
        public string isventilated { get; set; }
        public string forprofile { get; set; }
        public bool fromtype { get; set; }
    }

    public class CSClassification
    {
        public string classificationname { get; set; }
        public string classificationitemcode { get; set; }
        public string classificationsource { get; set; }
        public string classificationedition { get; set; }
        public string classificationeditiondate { get; set; }
        public string classificationitemname { get; set; }
        public string classificationitemlocation { get; set; }
        public bool fromtype { get; set; }
    }

    public class CSType
    {
        public string typeid { get; set; }
        public string ifctype { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int? ownerhistoryid { get; set; }
        public string applicableoccurence { get; set; }
        public string tag { get; set; }
        public string elementtype { get; set; }
        public string predefinedtype { get; set; }
        public string assemblyplace { get; set; }
        public string operationtype { get; set; }
        public string constructiontype { get; set; }
    }
}
