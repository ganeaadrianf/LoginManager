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
using LoginManager.Classes;
using PasswordUtility.PasswordGenerator;
using Microsoft.SqlServer.Management.Smo;

namespace LoginManager
{
    public partial class frmMain : Form
    {
        private string queryUsers = string.Empty;

        private string queryUserRoles = string.Empty;
        private string adminAplicConnectionString = string.Empty;
        private string aplicatieList = string.Empty;

        private string tipAccessList = string.Empty;

        private const int maxNumberOfServers = 10;
        private const int depHierarchyCount = 7;

        private List<string> depHierarchyQueries = new List<string>();

        private string updateUser = string.Empty;
        private string insertUser = string.Empty;
        private string insertUserRole = string.Empty;
        private string deleteUser = string.Empty;
        private string deleteUserRole = string.Empty;
        private string checkIfUserExists = string.Empty;
        private string checkIfLoginExists = string.Empty;
        private string checkIfUserMappedToRole = string.Empty;
        private string createLogin = string.Empty;
        private string resetLogin = string.Empty;
        private string dropLogin = string.Empty;

        private string createDBUserForLogin = string.Empty;
        private string addRoleMember = string.Empty;
        private string dropRoleMember = string.Empty;



        private Dictionary<string, string> sqlServerConnectionStrings = new Dictionary<string, string>();

        public List<DatabaseApplication> apps = new List<DatabaseApplication>();
        private List<Classes.DatabaseRole> access = new List<Classes.DatabaseRole>();


        public frmMain()

        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            WriteLog("Application started!");
            try
            {
                //throw new Exception("etst");
                lblTipAcces.Text = string.Empty;
                lblAplicatie.Text = string.Empty;
                btnDeleteRole.Visible = false;
                WriteLog("Loading config settings...");
                LoadConnectionStrings();
                adminAplicConnectionString = ConfigurationManager.ConnectionStrings["AdminAplicConnectionString"].ConnectionString;
                queryUsers = ConfigurationManager.AppSettings["QueryUsers"];
                queryUserRoles = ConfigurationManager.AppSettings["QueryUserRoles"];
                updateUser = ConfigurationManager.AppSettings["UpdateUser"];
                insertUser = ConfigurationManager.AppSettings["InsertUser"];
                insertUserRole = ConfigurationManager.AppSettings["InsertUserRole"];
                deleteUser = ConfigurationManager.AppSettings["DeleteUser"];
                deleteUserRole = ConfigurationManager.AppSettings["DeleteUserRole"];
                checkIfUserExists = ConfigurationManager.AppSettings["CheckIfUserExists"];
                checkIfLoginExists = ConfigurationManager.AppSettings["CheckIfLoginExists"];
                checkIfUserMappedToRole = ConfigurationManager.AppSettings["CheckIfUserMappedToRole"];
                createLogin = ConfigurationManager.AppSettings["CreateLogin"];
                resetLogin = ConfigurationManager.AppSettings["ResetLogin"];
                dropLogin = ConfigurationManager.AppSettings["DropLogin"];

                createDBUserForLogin = ConfigurationManager.AppSettings["CreateDBUserForLogin"];
                addRoleMember = ConfigurationManager.AppSettings["AddRoleMember"];
                dropRoleMember = ConfigurationManager.AppSettings["DropRoleMember"];


                aplicatieList = ConfigurationManager.AppSettings["AplicatieList"];
                tipAccessList = ConfigurationManager.AppSettings["TipAccesList"];

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
                WriteLog("Loading comboboxes...");
                LoadComboboxes();
                WriteLog("Loading people grid...");
                ReloadPeople();
            }
            catch (Exception xcp) {
                WriteLog(xcp.Message, 3);
            }
        }
        private void LoadConnectionStrings()
        {
            var numberOfSQLServers = Int32.Parse(ConfigurationManager.AppSettings["NumberOfSQLServers"]);
            for (int i = 0; i < numberOfSQLServers; i++)
            {
                sqlServerConnectionStrings.Add(string.Format("SQLSERVER{0}", i + 1), ConfigurationManager.ConnectionStrings[string.Format("SQLServer{0}", i + 1)].ConnectionString);
            }
        }

        private void ReloadPeople(string searchParam = null)
        {
            
            var queryUsers = string.Format(this.queryUsers, searchParam == null ? string.Empty : searchParam);

            using (var connection = new SqlConnection(adminAplicConnectionString))
            {
                var command = new SqlCommand(queryUsers, connection);
                WriteLog(string.Format("Opening connection to {0}...",adminAplicConnectionString));
                connection.Open();
                using (var reader = command.ExecuteReader())
                {

                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    
                    gridPeople.AutoGenerateColumns = true;
                    gridPeople.DataSource = dt;
                    gridPeople.Refresh();
                    WriteLog(string.Format("People loaded"));

                }
            }
        }

        private void ReloadRoles(string searchParam = null)
        {
            WriteLog("Loading permissions grid...");
            var qRoles = string.Format(queryUserRoles, searchParam == null ? string.Empty : searchParam);

            using (var connection = new SqlConnection(adminAplicConnectionString))
            {
                var command = new SqlCommand(qRoles, connection);
                WriteLog(string.Format("Opening connection to {0}...", adminAplicConnectionString));
                connection.Open();
                using (var reader = command.ExecuteReader())
                {

                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    gridRoles.AutoGenerateColumns = true;
                    gridRoles.DataSource = dt;
                    btnDeleteRole.Visible = dt.Rows.Count > 0;
                    if (dt.Rows.Count == 0)
                    {
                        lblAplicatie.Text = string.Empty;
                        lblTipAcces.Text = string.Empty;
                        lblServerInfo.Text = "";
                        lblLoginInfo.Text = "";
                        chkSpecialChars.Visible = false;
                        chkUpper.Visible = false;
                        chkUseDigits.Visible = false;
                        lblMinChars.Visible = false;
                        btnResetPass.Visible = false;
                        btnGeneratePassword.Visible = false;
                        txtMinLength.Visible = false;
                        txtPassword.Visible = false;
                        lblUserInRole.Visible = false;
                        btnDeleteLogin.Visible = false;
                        btnTestCredential.Visible = false;
                        btnDropRole.Visible = false;


                    }
                    else
                    {
                        chkSpecialChars.Visible = true;
                        chkUpper.Visible = true;
                        chkUseDigits.Visible = true;
                        lblMinChars.Visible = true;
                        //btnResetPass.Visible = true;
                        btnGeneratePassword.Visible = true;
                        txtMinLength.Visible = true;
                        txtPassword.Visible = true;
                        lblUserInRole.Visible = true;

                        //CheckSQLLogin(apps.Where(a => a.AppName == lblAplicatie.Text).First().ConnectionString);
                        //CheckDatabaseRole(apps.Where(a => a.AppName == lblAplicatie.Text).First().ConnectionString.Replace("Initial Catalog=master", "Initial Catalog =" + apps.Where(a => a.AppName == lblAplicatie.Text).First().DBName), access.Where(a => a.Display == lblTipAcces.Text).First().Value);
                    }
                    gridRoles.Refresh();
                    WriteLog("Permissions grid loaded");
                }
            }

        }
        private void CheckSQLLogin(string connString)
        {
            var qCheckLogin = string.Format(checkIfLoginExists, txtLogin.Text);

            using (var connection = new SqlConnection(connString))
            {
                var command = new SqlCommand(qCheckLogin, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lblLoginInfo.ForeColor = Color.Black;
                        lblLoginInfo.Text = "       Loginul exista pe serverul de mai sus.";
                        btnResetPass.Visible = true;
                        btnCreateLogin.Visible = false;
                        btnDeleteLogin.Visible = true;
                    }
                    else
                    {
                        lblLoginInfo.ForeColor = Color.Red;
                        lblLoginInfo.Text = "       Nu exista un login cu acest nume!!";
                        btnResetPass.Visible = false;
                        btnCreateLogin.Visible = true;
                        btnDeleteLogin.Visible = false;
                    }
                }
            }
        }


        private void CheckDatabaseRole(string connString, string dbRole)
        {
            var qCheckDBRole = string.Format(checkIfUserMappedToRole, txtLogin.Text, dbRole);

            using (var connection = new SqlConnection(connString))
            {
                var command = new SqlCommand(qCheckDBRole, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lblUserInRole.ForeColor = Color.Black;
                        lblUserInRole.Text = "       Userul este in rolul: " + dbRole;
                        btnAddLoginToRole.Visible = false;
                        btnDropRole.Visible = true;
                    }
                    else
                    {
                        lblUserInRole.ForeColor = Color.Red;
                        lblUserInRole.Text = "       Userul nu are rolul: " + dbRole;
                        btnAddLoginToRole.Visible = true;
                        btnDropRole.Visible = false;
                    }
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

            using (var connection = new SqlConnection(adminAplicConnectionString))
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
            DataTable dt;
            var qAplicatie = string.Format(aplicatieList);

            using (var connection = new SqlConnection(adminAplicConnectionString))
            {
                var command = new SqlCommand(qAplicatie, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {

                    dt = new DataTable();

                    dt.Load(reader);





                }
            }
            apps.Add(new DatabaseApplication() { AppName = "", DBName = "", DBServer = "" });

            foreach (DataRow row in dt.Rows)
            {
                apps.Add(new DatabaseApplication() { AppName = row["AppName"].ToString(), DBName = row["DBName"].ToString(), DBServer = row["DBServer"].ToString(), ConnectionString = sqlServerConnectionStrings[row["DBServer"].ToString().ToUpper()] });
            }


            //var emptyRow = dt.NewRow();
            //emptyRow["BazaDate"] = -1;
            //emptyRow["Aplicatie"] = string.Empty;
            //dt.Rows.Add(emptyRow);
            //var dv = new DataView(dt, "", "Aplicatie", DataViewRowState.CurrentRows);

            //cmbAplicatie.DataSource = dv;
            //cmbAplicatie.DisplayMember = "Aplicatie";
            //cmbAplicatie.ValueMember = "BazaDate";

            cmbAplicatie.DataSource = apps;
            cmbAplicatie.DisplayMember = "AppName";
            cmbAplicatie.ValueMember = "ConnectionInfo";


            cmbAplicatie.Refresh();
        }

        private void LoadTipAccesCombobox()
        {
            DataTable dt;
            var qtipAcces = string.Format(tipAccessList);

            using (var connection = new SqlConnection(adminAplicConnectionString))
            {
                var command = new SqlCommand(qtipAcces, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {

                    dt = new DataTable();

                    dt.Load(reader);


                }
            }
            access.Add(new Classes.DatabaseRole() { Display = "", Value = "-1" });
            foreach (DataRow row in dt.Rows)
            {
                access.Add(new Classes.DatabaseRole() { Value = row["DBRole"].ToString(), Display = row["Label"].ToString() });
            }
            //var emptyRow = dt.NewRow();
            //emptyRow["DBRole"] = -1;
            //emptyRow["Label"] = string.Empty;
            //dt.Rows.Add(emptyRow);
            //var dv = new DataView(dt, "", "Label", DataViewRowState.CurrentRows);

            cmbTipAcces.DataSource = access;
            cmbTipAcces.DisplayMember = "Display";
            cmbTipAcces.ValueMember = "Value";



            cmbTipAcces.Refresh();
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
            try
            {
                bool userExists = false;
                var queryUpdateUser = string.Format(updateUser,
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



                var qInsertUser = string.Format(insertUser, txtNume.Text,
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
                var qCheckUser = string.Format(checkIfUserExists, txtLogin.Text);

                using (var connection = new SqlConnection(adminAplicConnectionString))
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
                    WriteLog("User exists!");
                    WriteLog(queryUpdateUser);
                    if (MessageBox.Show(string.Format("Modificarile vor fi salvate!\nDoriti sa continuati?\n\n{0}", queryUpdateUser), "USER EXISTENT", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    {
                        using (var connection = new SqlConnection(adminAplicConnectionString))
                        {
                            var command = new SqlCommand(queryUpdateUser, connection);
                            connection.Open();
                            try
                            {
                                command.ExecuteNonQuery();
                                ReloadPeople();
                                WriteLog("OK!", 2);
                            }
                            catch (Exception xcp)
                            {
                                WriteLog(xcp.Message, 3);
                                MessageBox.Show(string.Format("Comanda e esuat!\n{0}\n{1}", xcp.Message, xcp.StackTrace), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }

                }
                else
                {
                    WriteLog("User not found!");
                    WriteLog(qInsertUser);
                    if (MessageBox.Show(string.Format("Modificarile vor fi salvate!\nDoriti sa continuati?\n\n{0}", qInsertUser), "USER NOU", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    {

                        using (var connection = new SqlConnection(adminAplicConnectionString))
                        {
                            var command = new SqlCommand(qInsertUser, connection);
                            connection.Open();
                            try
                            {
                                command.ExecuteNonQuery();
                                txtSearch.Text = txtLogin.Text;
                                ReloadPeople(txtLogin.Text);
                                WriteLog("OK!", 2);
                            }
                            catch (Exception xcp)
                            {
                                WriteLog(xcp.Message, 3);
                                MessageBox.Show(string.Format("Comanda e esuat!\n{0}\n{1}", xcp.Message, xcp.StackTrace), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                
                            }
                        }
                    }
                }
            }
            catch (Exception xcp) {
                WriteLog(xcp.Message, 3);
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
            WriteLog("Clear fields!");
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
            
            var qDeleteUser = string.Format(deleteUser, txtLogin.Text);
            if (MessageBox.Show(string.Format("Userul va fi sters ireversibil!\nSigur doriti sa continuati?\n\n{0}", qDeleteUser), "Delete user?", MessageBoxButtons.YesNo, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                WriteLog("Deleting user...");
                using (var connection = new SqlConnection(adminAplicConnectionString))
                {
                    var command = new SqlCommand(qDeleteUser, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        ReloadPeople();
                        WriteLog("OK!",2);
                    }
                    catch (Exception xcp)
                    {
                        WriteLog(xcp.Message, 3);
                        MessageBox.Show(string.Format("Comanda e esuat!\n{0}\n{1}", xcp.Message, xcp.StackTrace), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnDeleteRole_Click(object sender, EventArgs e)
        {
            //if (lblAplicatie.Text == string.Empty || lblTipAcces.Text == string.Empty)
            //return;
            var qDeleteUserRole = string.Format(deleteUserRole, txtLogin.Text, lblAplicatie.Text, lblTipAcces.Text);
            if (MessageBox.Show(string.Format("Rolul va fi sters ireversibil!\nSigur doriti sa continuati?\n\n{0}", qDeleteUserRole), "Delete Role?", MessageBoxButtons.YesNo, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {

                using (var connection = new SqlConnection(adminAplicConnectionString))
                {
                    var command = new SqlCommand(qDeleteUserRole, connection);
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
                btnDeleteRole.Visible = true;
                var app = apps.Where(a => a.AppName == lblAplicatie.Text).First();
                lblServerInfo.Text = "      " + app.ConnectionString;

                CheckSQLLogin(apps.Where(a => a.AppName == lblAplicatie.Text).First().ConnectionString);
                CheckDatabaseRole(apps.Where(a => a.AppName == lblAplicatie.Text).First().ConnectionString.Replace("Initial Catalog=master", "Initial Catalog =" + apps.Where(a => a.AppName == lblAplicatie.Text).First().DBName), access.Where(a => a.Display == lblTipAcces.Text).First().Value);


            }
            else
            {
                btnDeleteRole.Visible = true;
                lblServerInfo.Text = "";
            }

        }

        private void BtnAddRole_Click(object sender, EventArgs e)
        {
            if (((List<string>)cmbAplicatie.SelectedValue)[0] == string.Empty || cmbTipAcces.SelectedValue.ToString() == "-1")
            {
                MessageBox.Show("Selectati aplicatia si tipul de acces!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var qInsertUserRole = string.Format(insertUserRole, txtLogin.Text,
                                    cmbAplicatie.Text,
                                    cmbTipAcces.Text);
            if (MessageBox.Show(string.Format("Modificarile vor fi salvate!\nDoriti sa continuati?\n\n{0}", qInsertUserRole), "Adaugare rol", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {

                using (var connection = new SqlConnection(adminAplicConnectionString))
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

        private void BtnCreateLogin_Click(object sender, EventArgs e)
        {
            var qCreateLogin = string.Format(createLogin, txtLogin.Text,
                                  txtPassword.Text);
            CreateOrModifyLogin(qCreateLogin);
        }

        private void CreateOrModifyLogin(string qCreateLogin)
        {

            if (MessageBox.Show(string.Format("Comanda de mai jos va fi rulata!\nNotati parola inainte de a continu!\nDoriti sa continuati?\n\n{0}", qCreateLogin), "Creare login", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {

                using (var connection = new SqlConnection(lblServerInfo.Text))
                {
                    var command = new SqlCommand(qCreateLogin, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        btnCreateLogin.Visible = false;

                    }
                    catch (Exception xcp)
                    {
                        MessageBox.Show(string.Format("Comanda e esuat!\n{0}\n{1}", xcp.Message, xcp.StackTrace), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                ReloadRoles(txtLogin.Text);
            }
        }

        private void BtnResetPass_Click(object sender, EventArgs e)
        {

            var qResetLogin = string.Format(resetLogin, txtLogin.Text,
                      txtPassword.Text);
            CreateOrModifyLogin(qResetLogin);
        }

        private void BtnGeneratePassword_Click(object sender, EventArgs e)
        {
            try
            {
                txtPassword.Text = PasswordUtility.PasswordGenerator.PwGenerator.Generate(Int32.Parse(txtMinLength.Text), chkUpper.Checked, chkUseDigits.Checked, chkSpecialChars.Checked).ReadString().Replace("'","-");
            }
            catch (Exception) { }
        }

        private void BtnDeleteLogin_Click(object sender, EventArgs e)
        {
            var qDropLogin = string.Format(dropLogin, txtLogin.Text);
            if (MessageBox.Show(string.Format("Comanda de mai jos va fi rulata!\nLoginul va fi sters ireversibil!\nDoriti sa continuati?\n\n{0}", qDropLogin), "Steregere login", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {

                using (var connection = new SqlConnection(lblServerInfo.Text))
                {
                    var command = new SqlCommand(qDropLogin, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        btnCreateLogin.Visible = true;
                        btnDeleteLogin.Visible = false;
                        btnResetPass.Visible = false;


                    }
                    catch (Exception xcp)
                    {
                        MessageBox.Show(string.Format("Comanda e esuat!\n{0}\n{1}", xcp.Message, xcp.StackTrace), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                ReloadRoles(txtLogin.Text);
            }
        }

        private void BtnAddLoginToRole_Click(object sender, EventArgs e)
        {
            var qCreateDbUser = string.Format(createDBUserForLogin, txtLogin.Text);
            if (MessageBox.Show(string.Format("Comanda de mai jos va fi rulata!\nUserul se va crea in baza de date!\nDoriti sa continuati?\n\n{0}", qCreateDbUser), "Creare user", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {

                using (var connection = new SqlConnection(lblServerInfo.Text.Replace("Initial Catalog=master", "Initial Catalog =" + apps.Where(a => a.AppName == lblAplicatie.Text).First().DBName)))
                {
                    var command = new SqlCommand(qCreateDbUser, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();

                    }
                    catch (Exception xcp)
                    {
                        MessageBox.Show(string.Format("Comanda e esuat!\n{0}", xcp.Message), "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        AddDBRoleMember();
                    }
                }
                //ReloadRoles(txtLogin.Text);
            }
        }

        private void AddDBRoleMember()
        {
            var qAddRoleMember = string.Format(addRoleMember, access.Where(a => a.Display == lblTipAcces.Text).First().Value, txtLogin.Text);
            if (MessageBox.Show(string.Format("Comanda de mai jos va fi rulata!\nUserul va fi adaugat in rol!\nDoriti sa continuati?\n\n{0}", qAddRoleMember), "Adaugare user in rol", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {

                using (var connection = new SqlConnection(lblServerInfo.Text.Replace("Initial Catalog=master", "Initial Catalog =" + apps.Where(a => a.AppName == lblAplicatie.Text).First().DBName)))
                {
                    var command = new SqlCommand(qAddRoleMember, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        btnAddLoginToRole.Visible = false;
                        btnDropRole.Visible = true;
                        lblUserInRole.Text = "       Needs refresh!";

                    }
                    catch (Exception xcp)
                    {
                        MessageBox.Show(string.Format("Comanda e esuat!\n{0}", xcp.Message), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
                //ReloadRoles(txtLogin.Text);
            }
        }

        private void BtnDropRole_Click(object sender, EventArgs e)
        {
            var qDropRoleMember = string.Format(dropRoleMember, access.Where(a => a.Display == lblTipAcces.Text).First().Value, txtLogin.Text);
            if (MessageBox.Show(string.Format("Comanda de mai jos va fi rulata!\nUserul va fi sters din rol!\nDoriti sa continuati?\n\n{0}", qDropRoleMember), "Stergere user din rol", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {

                using (var connection = new SqlConnection(lblServerInfo.Text.Replace("Initial Catalog=master", "Initial Catalog =" + apps.Where(a => a.AppName == lblAplicatie.Text).First().DBName)))
                {
                    var command = new SqlCommand(qDropRoleMember, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        btnDropRole.Visible = false;
                        btnAddLoginToRole.Visible = true;
                        lblUserInRole.Text = "       Needs refresh!";
                       
                    }
                    catch (Exception xcp)
                    {
                        MessageBox.Show(string.Format("Comanda e esuat!\n{0}", xcp.Message), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }

            }
        }

        private void WriteLog(string message, int? type=null) {
            ListViewItem li = new ListViewItem();
            switch (type) {
                case 3:
                    li.ForeColor = Color.Red;
                    break;
                case 2:
                    li.ForeColor = Color.Green;
                    break;
                default:
                    li.ForeColor = Color.Black;
                    break;

            }
               
           
            li.Text = message;
            
            logList.Items.Insert(0,li);
        }
    }
}
