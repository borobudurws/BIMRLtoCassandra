using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BIMRL;
using BIMRL.OctreeLib;

namespace BIMRLtoCassandra
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            BIMRLCommon BIMRLCommonRef = new BIMRLCommon();

            InitializeComponent();
            
            // Connect to Oracle DB
            DBOperation.refBIMRLCommon = BIMRLCommonRef;      // important to ensure DBoperation has reference to this object!!
            if (DBOperation.Connect() == null)
            {
                BIMRLErrorDialog erroDlg = new BIMRLErrorDialog(BIMRLCommonRef);
                erroDlg.ShowDialog();
                return;
            }

            BIMRLQueryModel _qModel = new BIMRLQueryModel(BIMRLCommonRef);
            List<BIMRLFedModel> fedModels = new List<BIMRLFedModel>();
            fedModels = _qModel.getFederatedModels();

            DataGrid_Oracle.AutoGenerateColumns = true;
            DataGrid_Oracle.IsReadOnly = true;
            DataGrid_Oracle.ItemsSource = fedModels;
            DataGrid_Oracle.MinRowHeight = 20;
            Button_Copy.IsEnabled = false;

            DataGrid_Cassandra.IsReadOnly = true;
            DataGrid_Cassandra.AutoGenerateColumns = true;
            DataGrid_Cassandra.MinRowHeight = 20;

            QueryCassDB qCDB = new QueryCassDB();
            List<BIMRLFedModel> modelList = qCDB.getCassFedModels();
            DataGrid_Cassandra.ItemsSource = modelList;
        }

        private void DataGrid_Oracle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<BIMRLFedModel> selFedModels = DataGrid_Oracle.SelectedItems as List<BIMRLFedModel>;
            if (selFedModels.Count == 0)
                return;

            BIMRL2Cassandra BIMRL2Cass = new BIMRL2Cassandra();
            foreach (BIMRLFedModel selFedModel in selFedModels)
            {
                BIMRL2Cass.copyData(selFedModel.FederatedID, selFedModel.ProjectNumber, selFedModel.ProjectName);
            }
        }


    }
}
