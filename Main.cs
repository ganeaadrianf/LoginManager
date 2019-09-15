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
        public string QueryUsers { get; set; }

        public string QueryUserRoles { get; set; }
        public string AdminAplicConnectionString { get; set; }
        public string AplicatieList { get; set; }

        public string TipAccessList { get; set; }

        private const int maxNumberOfServers = 10;
        private const int depHierarchyCount = 7;

        public List<string> depHierarchyQueries { get; set; }

        public string UpdateUser { get; set; }
        public string InsertUser { get; set; }
        public string InsertUserRole { get; set; }
        public string DeleteUser { get; set; }
        public string DeleteUserRole { get; set; }
        public string CheckIfUserExists { get; set; }
        public frmMain()

        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            AdminAplicConnectionString = ConfigurationManager.ConnectionStrings["AdminAplicConnectionString"].ConnectionString;
            QueryUsers = ConfigurationManager.AppSettings["QueryUsers"];
            QueryUserRoles = ConfigurationManager.AppSettings["QueryUserRoles"];
            UpdateUser = ConfigurationManager.AppSettings["UpdateUser"];
            InsertUser = ConfigurationManager.AppSettings["InsertUser"];
            InsertUserRole = ConfigurationManager.AppSettings["InsertUserRole"];
            DeleteUser = ConfigurationManager.AppSettings["DeleteUser"];
            DeleteUserRole = ConfigurationManager.AppSettings["DeleteUserRole"];
            CheckIfUserExists = ConfigurationManager.AppSettings["CheckIfUserExists"];

            AplicatieList = ConfigurationManager.AppSettings["AplicatieList"];
            TipAccessList= ConfigurationManager.AppSettings["TipAccesList"];

            depHierarchyQueries = new List<string>();
            for (int i = 1; i <= depHierarchyCount; i++)
            {
                try
                {
                    depHierarchyQueries.Add(ConfigurationManager.AppSettings[String.Format("C{0}List", i)]);
                }
                catch (Exception)
                { //do nothing
                }
            }

            LoadComboboxes();
            ReloadPeople();
        }

        private void ReloadPeople(string searchParam = null)
        {
            var queryUsers = string.Format(QueryUsers, searchParam == null ? string.Empty : searchParam);

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

        private void ReloadRoles(string searchParam = null)
        {
            var qRoles = string.Format(QueryUserRoles, searchParam == null ? string.Empty : searchParam);

            using (var connection = new SqlConnection(AdminAplicConnectionString))
            {
                var command = new SqlCommand(qRoles, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {

                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    gridRoles.AutoGenerateColumns = true;
                    gridRoles.DataSource = dt;
                    gridRoles.Refresh();

                }
            }
        }


        private void LoadComboboxes()
        {
            for (int i = 0; i < depHierarchyQueries.Count; i++)
            {
                LoadCombobox(i);
            }
            LoadAplicatieCombobox();
            LoadTipAccesCombobox();
        }
        private void LoadCombobox(int index)
        {

            var queryComboLists = string.Format(depHierarchyQueries[index]);

            using (var connection = new SqlConnection(AdminAplicConnectionString))
            {
                var command = new SqlCommand(queryComboLists, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {

                    DataTable dt = new DataTable();

                    dt.Load(reader);
                    var emptyRow = dt.NewRow();
                    emptyRow["Valoare"] = -1;
                    emptyRow["Denumire"] = string.Empty;
                    dt.Rows.Add(emptyRow);
                    var dv = new DataView(dt, "", "Denumire", DataViewRowState.CurrentRows);
                    var combo = (ComboBox)(this.Controls.Find(string.Format("cmbC{0}", index + 1), true)[0]);
                    combo.DataSource = dv;
                    combo.DisplayMember = "Denumire";
                    combo.ValueMember = "Valoare";



                    combo.Refresh();

                }
            }
        }


        private void LoadAplicatieCombobox()
        {

            var qAplicatie = string.Format(AplicatieList);

            using (var connection = new SqlConnection(AdminAplicConnectionString))
            {
                var command = new SqlCommand(qAplicatie, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {

                    DataTable dt = new DataTable();

                    dt.Load(reader);
                    var emptyRow = dt.NewRow();
                    emptyRow["BazaDate"] = -1;
                    emptyRow["Aplicatie"] = string.Empty;
                    dt.Rows.Add(emptyRow);
                    var dv = new DataView(dt, "", "Aplicatie", DataViewRowState.CurrentRows);
                    
                    cmbAplicatie.DataSource = dv;
                    cmbAplicatie.DisplayMember = "Aplicatie";
                    cmbAplicatie.ValueMember = "BazaDate";



                    cmbAplicatie.Refresh();

                }
            }
        }

        private void LoadTipAccesCombobox()
        {

            var qtipAcces = string.Format(TipAccessList);

            using (var connection = new SqlConnection(AdminAplicConnectionString))
            {
                var command = new SqlCommand(qtipAcces, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {

                    DataTable dt = new DataTable();

                    dt.Load(reader);
                    var emptyRow = dt.NewRow();
                    emptyRow["DBRole"] = -1;
                    emptyRow["Label"] = string.Empty;
                    dt.Rows.Add(emptyRow);
                    var dv = new DataView(dt, "", "Label", DataViewRowState.CurrentRows);

                    cmbTipAcces.DataSource = dv;
                    cmbTipAcces.DisplayMember = "Label";
                    cmbTipAcces.ValueMember = "DBRole";



                    cmbTipAcces.Refresh();

                }
            }
        }

        private void TxtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ReloadPeople(txtSearch.Text);
            }
        }

        private void GridPeople_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                gridPeople.CurrentRow.Selected = true;
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            bool userExists = false;
            var queryUpdateUser = string.Format(UpdateUser,
                                                txtNume.Text,
                                                txtPrenume.Text,
                                                txtCNP.Text,
                                                txtLogin.Text,
                                                cmbC1.SelectedValue,
                                                cmbC2.SelectedValue,
                                                cmbC3.SelectedValue,
                                                cmbC4.SelectedValue,
                                                cmbC5.SelectedValue,
                                                cmbC6.SelectedValue,
                                                cmbC7.SelectedValue
                                                );



            var qInsertUser = string.Format(InsertUser, txtNume.Text,
                                                txtPrenume.Text,
                                                txtCNP.Text,
                                                txtLogin.Text,
                                                cmbC1.SelectedValue,
                                                cmbC2.SelectedValue,
                                                cmbC3.SelectedValue,
                                                cmbC4.SelectedValue,
                                                cmbC5.SelectedValue,
                                                cmbC6.SelectedValue,
                                                cmbC7.SelectedValue);
            var qCheckUser = string.Format(CheckIfUserExists, txtLogin.Text);

            using (var connection = new SqlConnection(AdminAplicConnectionString))
            {
                var command = new SqlCommand(qCheckUser, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        userExists = true;

                    }
                }
            }

            if (userExists)
            {
                if (MessageBox.Show(string.Format("Modificarile vor fi salvate!\nDoriti sa continuati?\n\n{0}", queryUpdateUser), "USER EXISTENT", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    using (var connection = new SqlConnection(AdminAplicConnectionString))
                    {
                        var command = new SqlCommand(queryUpdateUser, connection);
                        connection.Open();
                        try
                        {
                            command.ExecuteNonQuery();
                            ReloadPeople();
                        }
                        catch (Exception xcp)
                        {
                            MessageBox.Show(string.Format("Comanda e esuat!\n{0}\n{1}", xcp.Message, xcp.StackTrace), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

            }
            else
            {
                if (MessageBox.Show(string.Format("Modificarile vor fi salvate!\nDoriti sa continuati?\n\n{0}", qInsertUser), "USER NOU", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {

                    using (var connection = new SqlConnection(AdminAplicConnectionString))
                    {
                        var command = new SqlCommand(qInsertUser, connection);
                        connection.Open();
                        try
                        {
                            command.ExecuteNonQuery();
                            ReloadPeople();
                        }
                        catch (Exception xcp)
                        {
                            MessageBox.Show(string.Format("Comanda e esuat!\n{0}\n{1}", xcp.Message, xcp.StackTrace), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }

        }

        private void GridPeople_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                //gridPeople.CurrentRow.Selected = true;
                txtCNP.Text = gridPeople.Rows[e.RowIndex].Cells["CNP"].FormattedValue.ToString();
                txtNume.Text = gridPeople.Rows[e.RowIndex].Cells["Nume"].FormattedValue.ToString();
                txtPrenume.Text = gridPeople.Rows[e.RowIndex].Cells["Prenume"].FormattedValue.ToString();
                txtLogin.Text = gridPeople.Rows[e.RowIndex].Cells["Login"].FormattedValue.ToString();
                try
                {
                    for (int i = 1; i <= depHierarchyCount; i++)
                    {
                        var combo = (ComboBox)(this.Controls.Find(string.Format("cmbC{0}", i), true)[0]);
                        if (!string.IsNullOrEmpty(gridPeople.Rows[e.RowIndex].Cells[string.Format("C{0}", i)].FormattedValue.ToString()))
                            combo.SelectedValue = gridPeople.Rows[e.RowIndex].Cells[string.Format("C{0}", i)].FormattedValue.ToString();
                        else
                            combo.SelectedValue = -1;

                    }

                }
                catch (Exception) { }
                ReloadRoles(gridPeople.Rows[e.RowIndex].Cells["Login"].FormattedValue.ToString());

            }

        }



        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtCNP.Text = string.Empty;
            txtNume.Text = string.Empty;
            txtPrenume.Text = string.Empty;
            txtLogin.Text = string.Empty;
            cmbC1.SelectedValue = -1;
            cmbC2.SelectedValue = -1;
            cmbC3.SelectedValue = -1;
            cmbC4.SelectedValue = -1;
            cmbC5.SelectedValue = -1;
            cmbC6.SelectedValue = -1;
            cmbC7.SelectedValue = -1;
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var qDeleteUser = string.Format(DeleteUser, txtLogin.Text);
            if (MessageBox.Show(string.Format("Userul va fi sters ireversibil!\nSigur doriti sa continuati?\n\n{0}", qDeleteUser), "Delete user?", MessageBoxButtons.YesNo, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {

                using (var connection = new SqlConnection(AdminAplicConnectionString))
                {
                    var command = new SqlCommand(qDeleteUser, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        ReloadPeople();
                    }
                    catch (Exception xcp)
                    {
                        MessageBox.Show(string.Format("Comanda e esuat!\n{0}\n{1}", xcp.Message, xcp.StackTrace), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnDeleteRole_Click(object sender, EventArgs e)
        {
            var qDeleteUserRole = string.Format(DeleteUserRole, txtLogin.Text, lblAplicatie.Text, lblTipAcces.Text);
            if (MessageBox.Show(string.Format("Rolul va fi sters ireversibil!\nSigur doriti sa continuati?\n\n{0}", qDeleteUserRole), "Delete Role?", MessageBoxButtons.YesNo, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {

                using (var connection = new SqlConnection(AdminAplicConnectionString))
                {
                    var command = new SqlCommand(qDeleteUserRole, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        ReloadPeople();
                    }
                    catch (Exception xcp)
                    {
                        MessageBox.Show(string.Format("Comanda e esuat!\n{0}\n{1}", xcp.Message, xcp.StackTrace), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void GridRoles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                gridRoles.CurrentRow.Selected = true;
            }
        }

        private void GridRoles_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {

                lblAplicatie.Text = gridRoles.Rows[e.RowIndex].Cells["Aplicatie"].FormattedValue.ToString();
                lblTipAcces.Text = gridRoles.Rows[e.RowIndex].Cells["TipAcces"].FormattedValue.ToString();


            }

        }

        private void BtnAddRole_Click(object sender, EventArgs e)
        {
            var qInsertUserRole = string.Format(InsertUserRole, txtLogin.Text,
                                    cmbAplicatie.Text,
                                    cmbTipAcces.Text) ;
            if (MessageBox.Show(string.Format("Modificarile vor fi salvate!\nDoriti sa continuati?\n\n{0}", qInsertUserRole), "Adaugare rol", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {

                using (var connection = new SqlConnection(AdminAplicConnectionString))
                {
                    var command = new SqlCommand(qInsertUserRole, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        ReloadRoles(txtLogin.Text);
                    }
                    catch (Exception xcp)
                    {
                        MessageBox.Show(string.Format("Comanda e esuat!\n{0}\n{1}", xcp.Message, xcp.StackTrace), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }
    }
}
