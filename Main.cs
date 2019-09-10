using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;

namespace LoginManager
{
    public partial class frmMain : Form
    {
        public string QueryUsersString { get; set; }
        public string AdminAplicConnectionString { get; set; }

        private const int maxNumberOfServers = 10;
        private const int depHierarchyCount = 7;

        public List<string> depHierarchyQueries { get; set; }

        public frmMain()

        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            AdminAplicConnectionString = ConfigurationManager.ConnectionStrings["AdminAplicConnectionString"].ConnectionString;
            QueryUsersString = ConfigurationManager.AppSettings["QueryUsers"];
            depHierarchyQueries = new List<string>();
            for (int i = 1; i <= depHierarchyCount; i++)
            {
                try
                {
                    depHierarchyQueries.Add(ConfigurationManager.AppSettings[String.Format("C{0}List", i)]);
                }
                catch (Exception) { //do nothing
                }
            }

            LoadComboboxes();
            ReloadGrid();
        }

        private void ReloadGrid(string searchParam = null)
        {
            var queryUsers = string.Format(QueryUsersString, searchParam == null ? string.Empty : searchParam);

            using (var connection = new SqlConnection(AdminAplicConnectionString))
            {
                var command = new SqlCommand(queryUsers, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {

                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    gridPeople.AutoGenerateColumns = true;
                    gridPeople.DataSource = dt;
                    gridPeople.Refresh();

                }
            }
        }


        private void LoadComboboxes() {
            for (int i = 0; i < depHierarchyQueries.Count; i++)
            {
                LoadCombobox(i);
            }
           }
        private void LoadCombobox(int index) {

            var queryComboLists= string.Format(depHierarchyQueries[index]);

            using (var connection = new SqlConnection(AdminAplicConnectionString))
            {
                var command = new SqlCommand(queryComboLists, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {

                    DataTable dt = new DataTable();
                   
                    dt.Load(reader);
                    var emptyRow = dt.NewRow();
                    emptyRow["Valoare"] = 0;
                    emptyRow["Denumire"] = string.Empty;
                    dt.Rows.Add(emptyRow);
                    var dv = new DataView(dt, "", "Denumire", DataViewRowState.CurrentRows);
                    var combo=(ComboBox)(this.Controls.Find(string.Format("cmbC{0}", index+1),true)[0]);
                    combo.DataSource = dv;
                    combo.DisplayMember = "Denumire";
                    combo.ValueMember = "Valoare";

                    

                    combo.Refresh();
                        
                }
            }
        }

        private void TxtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ReloadGrid(txtSearch.Text);
            }
        }

        private void GridPeople_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (gridPeople.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null) {
                gridPeople.CurrentRow.Selected = true;
                txtCNP.Text = gridPeople.Rows[e.RowIndex].Cells["CNP"].FormattedValue.ToString();
                txtNume.Text = gridPeople.Rows[e.RowIndex].Cells["Nume"].FormattedValue.ToString();
                txtPrenume.Text = gridPeople.Rows[e.RowIndex].Cells["Prenume"].FormattedValue.ToString();
                txtLogin.Text = gridPeople.Rows[e.RowIndex].Cells["Login"].FormattedValue.ToString();

            }
        }
    }
}
