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
using System.IO;
using System.Diagnostics;

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
        private const int depCount = 7;

        private List<string> depHierarchyQueries = new List<string>();

        private string depHierarchyQuery = string.Empty;
        private string updateUser = string.Empty;
        private string insertUser = string.Empty;
        private string insertUserRole = string.Empty;
        private string deleteUser = string.Empty;
        private string deleteUserRole = string.Empty;
        private string revokeUserRole = string.Empty;
        private string checkIfUserExists = string.Empty;
        private string checkIfLoginExists = string.Empty;
        private string checkIfUserMappedToRole = string.Empty;
        private string createLogin = string.Empty;
        private string resetLogin = string.Empty;
        private string dropLogin = string.Empty;

        private string createDBUserForLogin = string.Empty;
        private string addRoleMember = string.Empty;
        private string dropRoleMember = string.Empty;
        private string alterUserWithLogin = string.Empty;

        private static string logFileFormat = "loginManager_{0}.txt";
        private string logFilename = String.Format(logFileFormat, System.DateTime.Now.ToString("dd.MM.yyyy HH_mm_ss"));


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
                revokeUserRole = ConfigurationManager.AppSettings["RevokeUserRole"];
                checkIfUserExists = ConfigurationManager.AppSettings["CheckIfUserExists"];
                checkIfLoginExists = ConfigurationManager.AppSettings["CheckIfLoginExists"];
                checkIfUserMappedToRole = ConfigurationManager.AppSettings["CheckIfUserMappedToRole"];
                createLogin = ConfigurationManager.AppSettings["CreateLogin"];
                resetLogin = ConfigurationManager.AppSettings["ResetLogin"];
                dropLogin = ConfigurationManager.AppSettings["DropLogin"];

                createDBUserForLogin = ConfigurationManager.AppSettings["CreateDBUserForLogin"];
                addRoleMember = ConfigurationManager.AppSettings["AddRoleMember"];
                dropRoleMember = ConfigurationManager.AppSettings["DropRoleMember"];
                alterUserWithLogin = ConfigurationManager.AppSettings["AlterUserWithLogin"];


                aplicatieList = ConfigurationManager.AppSettings["AplicatieList"];
                tipAccessList = ConfigurationManager.AppSettings["TipAccesList"];

                //depHierarchyQueries = new List<string>();
                //for (int i = 1; i <= depHierarchyCount; i++)
                //{
                //    try
                //    {
                //        depHierarchyQueries.Add(ConfigurationManager.AppSettings[String.Format("C{0}List", i)]);
                //    }
                //    catch (Exception)
                //    { //do nothing
                //    }
                //}
                depHierarchyQuery=ConfigurationManager.AppSettings[String.Format("CList")];
                WriteLog("Loading comboboxes...");
                LoadComboboxes();
                WriteLog("Loading people grid...");
                ReloadPeople();
            }
            catch (Exception xcp)
            {
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
            var queryUsers = string.Empty;
            List<int> vals = new List<int>();
            int result = -1;
            if (Int32.TryParse(textBox1.Text, out result))
                vals.Add(result);
            else
                vals.Add(-1);

            if (Int32.TryParse(textBox2.Text, out result))
                vals.Add(result);
            else
                vals.Add(-1);

            if (Int32.TryParse(textBox3.Text, out result))
                vals.Add(result);
            else
                vals.Add(-1);

            if (Int32.TryParse(textBox4.Text, out result))
                vals.Add(result);
            else
                vals.Add(-1);

            if (Int32.TryParse(textBox5.Text, out result))
                vals.Add(result);
            else
                vals.Add(-1);

            if (Int32.TryParse(textBox6.Text, out result))
                vals.Add(result);
            else
                vals.Add(-1);

            if (Int32.TryParse(textBox7.Text, out result))
                vals.Add(result);
            else
                vals.Add(-1);

            queryUsers = string.Format(this.queryUsers, searchParam == null ? string.Empty : searchParam,
               vals[0],
               vals[1],
               vals[2],
               vals[3],
               vals[4],
               vals[5],
               vals[6],
                textBox1.Text,
                textBox2.Text,
                textBox3.Text,
                textBox4.Text,
                textBox5.Text,
                textBox6.Text,
                textBox7.Text
                );
            //MessageBox.Show(queryUsers);
            WriteLog(queryUsers, 2);
            using (var connection = new SqlConnection(adminAplicConnectionString))
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
                        //btnTestCredential.Visible = true;
                    }
                    else
                    {
                        lblLoginInfo.ForeColor = Color.Red;
                        lblLoginInfo.Text = "       Nu exista un login cu acest nume!!";
                        btnResetPass.Visible = false;
                        btnCreateLogin.Visible = true;
                        btnDeleteLogin.Visible = false;
                        btnTestCredential.Visible = false;
                    }
                }
            }
        }


        private void CheckDatabaseRole(string connString, string dbRole)
        {
            var qCheckDBRole = string.Format(checkIfUserMappedToRole, txtLogin.Text, dbRole);
            WriteLog(qCheckDBRole, 1);
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
            //for (int i = 0; i < depHierarchyQueries.Count; i++)
            //{
            Task task = Task.Factory.StartNew(() =>
            {
                // Background work
                LoadCombobox();
            }).ContinueWith((t)=> {
                //GridPeople_RowEnter("force click", new DataGridViewCellEventArgs(0, 0));
                WriteLog("Listele de unitati au fost incarcate!",5);
                pictureBox1.Visible = false;
            });

            
            //}
            LoadAplicatieCombobox();
            LoadTipAccesCombobox();
        }
        private void LoadCombobox()
        {
            DataTable dt;
            var queryComboLists = string.Format(depHierarchyQuery);
            WriteLog(queryComboLists, 2);
            using (var connection = new SqlConnection(adminAplicConnectionString))
            {
                
                var command = new SqlCommand(queryComboLists, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {

                    dt = new DataTable();

                    dt.Load(reader);


                }
            }
           
            var emptyRow = dt.NewRow();
            emptyRow["Valoare"] = -1;
            emptyRow["Denumire"] = string.Empty;
            dt.Rows.Add(emptyRow);


           

            for (int i = 0; i < 7; i++)
            {
                var dv = new DataView(dt, "", "Denumire", DataViewRowState.CurrentRows);
                var combo = (ComboBox)(this.Controls.Find(string.Format("cmbC{0}", i + 1), true)[0]);
                combo.DataSource = dv;
                combo.DisplayMember = "Denumire";
                combo.ValueMember = "Valoare";
                //combo.Refresh();
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
                                                    txtTelContact.Text,
                                                    txtCNP.Text,
                                                    txtLogin.Text,
                                                    txtc1.Text == string.Empty ? "0" :  txtc1.Text,
                                                    txtc2.Text == string.Empty ? "0" : txtc2.Text,
                                                    txtc3.Text == string.Empty ? "0" : txtc3.Text,
                                                    txtc4.Text == string.Empty ? "0" : txtc4.Text,
                                                    txtc5.Text == string.Empty ? "0" : txtc5.Text,
                                                    txtc6.Text == string.Empty ? "0" : txtc6.Text,
                                                    txtc7.Text == string.Empty ? "0" : txtc7.Text,
                                                    txtEmail.Text,
                                                    txtUnitatea.Text
                                                    );



                var qInsertUser = string.Format(insertUser, txtNume.Text,
                                                    txtTelContact.Text,
                                                    txtCNP.Text,
                                                    txtLogin.Text,
                                                    txtc1.Text,
                                                    txtc2.Text,
                                                    txtc3.Text,
                                                    txtc4.Text,
                                                    txtc5.Text,
                                                    txtc6.Text,
                                                    txtc7.Text,
                                                    txtEmail.Text,
                                                    txtUnitatea.Text
                                                    );
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
                    WriteLog(queryUpdateUser, 1);
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
            catch (Exception xcp)
            {
                WriteLog(xcp.Message, 3);
            }

        }

        private void GridPeople_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                //gridPeople.CurrentRow.Selected = true;
                txtCNP.Text = gridPeople.Rows[e.RowIndex].Cells["cnp"].FormattedValue.ToString();
                txtNume.Text = gridPeople.Rows[e.RowIndex].Cells["nume_prenume"].FormattedValue.ToString();
                txtTelContact.Text = gridPeople.Rows[e.RowIndex].Cells["tel_contact"].FormattedValue.ToString();
                txtLogin.Text = gridPeople.Rows[e.RowIndex].Cells["nume_user"].FormattedValue.ToString();
                txtEmail.Text = gridPeople.Rows[e.RowIndex].Cells["mail_intranet"].FormattedValue.ToString();
                txtUnitatea.Text = gridPeople.Rows[e.RowIndex].Cells["unitatea"].FormattedValue.ToString();
                try
                {
                    for (int i = 1; i <= depCount; i++)
                    {
                        var text = (TextBox)(this.Controls.Find(string.Format("txtc{0}", i), true)[0]);
                        if (!string.IsNullOrEmpty(gridPeople.Rows[e.RowIndex].Cells[string.Format("C{0}", i)].FormattedValue.ToString()))
                            text.Text = gridPeople.Rows[e.RowIndex].Cells[string.Format("c{0}", i)].FormattedValue.ToString();
                        else
                            text.Text = "0";

                    }

                }
                catch (Exception) { }
                ReloadRoles(gridPeople.Rows[e.RowIndex].Cells["nume_user"].FormattedValue.ToString());

            }


        }



        private void BtnClear_Click(object sender, EventArgs e)
        {
            WriteLog("Clear fields!");
            txtCNP.Text = string.Empty;
            txtNume.Text = string.Empty;
            txtTelContact.Text = string.Empty;
            txtLogin.Text = string.Empty;
            txtc1.Text = "0";
            txtc2.Text = "0";
            txtc3.Text = "0";
            txtc4.Text = "0";
            txtc5.Text = "0";
            txtc6.Text = "0";
            txtc7.Text = "0";
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {

            var qDeleteUser = string.Format(deleteUser, txtLogin.Text);
            if (MessageBox.Show(string.Format("Userul va fi sters ireversibil!\nSigur doriti sa continuati?\n\n{0}", qDeleteUser), "Delete user?", MessageBoxButtons.YesNo, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                WriteLog("Deleting user: " + qDeleteUser, 1);
                using (var connection = new SqlConnection(adminAplicConnectionString))
                {
                    var command = new SqlCommand(qDeleteUser, connection);
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

        private void BtnDeleteRole_Click(object sender, EventArgs e)
        {
            //if (lblAplicatie.Text == string.Empty || lblTipAcces.Text == string.Empty)
            //return;

            var qDeleteUserRole = string.Format(deleteUserRole, txtLogin.Text, lblAplicatie.Text, lblTipAcces.Text);
            if (MessageBox.Show(string.Format("Rolul va fi sters ireversibil!\nSigur doriti sa continuati?\n\n{0}", qDeleteUserRole), "Delete Role?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                WriteLog("Stergere rol din admin aplic!");
                WriteLog(qDeleteUserRole, 1);
                using (var connection = new SqlConnection(adminAplicConnectionString))
                {
                    var command = new SqlCommand(qDeleteUserRole, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        ReloadRoles(txtLogin.Text);
                        WriteLog("OK!", 2);
                    }
                    catch (Exception xcp)
                    {
                        WriteLog(qDeleteUserRole, 1);
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

                lblAplicatie.Text = gridRoles.Rows[e.RowIndex].Cells["aplicatia"].FormattedValue.ToString();
                lblTipAcces.Text = gridRoles.Rows[e.RowIndex].Cells["drept_solicitat"].FormattedValue.ToString();
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

            try
            {
                if (((List<string>)cmbAplicatie.SelectedValue)[0] == string.Empty || cmbTipAcces.SelectedValue.ToString() == "-1")
                {
                    MessageBox.Show("Selectati aplicatia si tipul de acces!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                WriteLog("Salvare permisiune in admin aplic...");
                var qInsertUserRole = string.Format(insertUserRole, txtLogin.Text,
                                        cmbAplicatie.Text,
                                        cmbTipAcces.Text);
                WriteLog(qInsertUserRole, 1);
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
            catch (Exception xcp)
            {
                WriteLog(xcp.Message, 3);
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
                WriteLog("Modificare login");
                WriteLog(qCreateLogin, 1);
                using (var connection = new SqlConnection(lblServerInfo.Text))
                {
                    var command = new SqlCommand(qCreateLogin, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        btnCreateLogin.Visible = false;
                        WriteLog("OK!", 2);

                    }
                    catch (Exception xcp)
                    {
                        WriteLog(xcp.Message, 3);
                        MessageBox.Show(string.Format("Comanda e esuat!\n{0}", xcp.Message), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                txtPassword.Text = PasswordUtility.PasswordGenerator.PwGenerator.Generate(Int32.Parse(txtMinLength.Text), chkUpper.Checked, chkUseDigits.Checked, chkSpecialChars.Checked).ReadString().Replace("'", "-").Replace(";", "_");
                WriteLog(txtPassword.Text, 4);
            }
            catch (Exception)
            {
            }
        }

        private void BtnDeleteLogin_Click(object sender, EventArgs e)
        {
            var qDropLogin = string.Format(dropLogin, txtLogin.Text);
            if (MessageBox.Show(string.Format("Comanda de mai jos va fi rulata!\nLoginul va fi sters ireversibil!\nDoriti sa continuati?\n\n{0}", qDropLogin), "Steregere login", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                WriteLog("Stergere login sql...");
                WriteLog(qDropLogin, 1);
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
                        WriteLog("OK!");


                    }
                    catch (Exception xcp)
                    {
                        WriteLog(xcp.Message, 3);
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
                WriteLog("Creare user db...");
                WriteLog(qCreateDbUser, 1);

                using (var connection = new SqlConnection(lblServerInfo.Text.Replace("Initial Catalog=master", "Initial Catalog =" + apps.Where(a => a.AppName == lblAplicatie.Text).First().DBName)))
                {
                    var command = new SqlCommand(qCreateDbUser, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        WriteLog("OK!", 2);

                    }
                    catch (Exception xcp)
                    {
                        WriteLog("userul nu a fost creat, probabil exista" + xcp.Message, 3);
                        //MessageBox.Show(string.Format("Comanda e esuat!\n{0}", xcp.Message), "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        AlterUserWithLoginIfExists();
                    }
                    finally
                    {
                        AddDBRoleMember();
                    }
                }
                //ReloadRoles(txtLogin.Text);
            }
        }

        private void AlterUserWithLoginIfExists()
        {
            var qAlterUSer = string.Format(alterUserWithLogin, txtLogin.Text);
            if (MessageBox.Show(string.Format("Comanda de mai jos va fi rulata!\nUserul se va crea in baza de date!\nDoriti sa continuati?\n\n{0}", qAlterUSer), "alter user", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                WriteLog("Creare user db...");
                WriteLog(qAlterUSer, 1);

                using (var connection = new SqlConnection(lblServerInfo.Text.Replace("Initial Catalog=master", "Initial Catalog =" + apps.Where(a => a.AppName == lblAplicatie.Text).First().DBName)))
                {
                    var command = new SqlCommand(qAlterUSer, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        WriteLog("OK!", 2);

                    }
                    catch (Exception xcp)
                    {
                        WriteLog("userul nu a fost modificat" + xcp.Message, 3);
                        //MessageBox.Show(string.Format("Comanda e esuat!\n{0}", xcp.Message), "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                WriteLog("Creare role db...");
                WriteLog(qAddRoleMember, 1);

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
                        WriteLog("OK!", 2);

                    }
                    catch (Exception xcp)
                    {
                        WriteLog(xcp.Message, 3);
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
                WriteLog("Stergere rol db...");
                WriteLog(qDropRoleMember, 1);
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
                        WriteLog("OK!", 2);

                    }
                    catch (Exception xcp)
                    {
                        WriteLog(xcp.Message, 3);
                        MessageBox.Show(string.Format("Comanda e esuat!\n{0}", xcp.Message), "Eror!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }

            }
        }

        private void WriteLog(string message, int? type = null)
        {
            ListViewItem li = new ListViewItem();

            switch (type)
            {
                case 5:
                    li.ForeColor = Color.Purple;
                    break;
                case 4:
                    li.ForeColor = Color.Orange;
                    break;
                case 3:
                    li.ForeColor = Color.Red;
                    break;
                case 2:
                    li.ForeColor = Color.Green;
                    break;
                case 1:
                    li.ForeColor = Color.Blue;
                    break;
                default:
                    li.ForeColor = Color.Black;
                    break;

            }


            li.Text = message;

            logList.Items.Insert(0, li);

            try
            {
                File.AppendAllText(logFilename, string.Format("{1} - {0}\r\n\r\n",message, System.DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")));
            }
            catch (Exception xcp)
            {
                MessageBox.Show("Nu s-a putut salva informatia in fisierul de output: " + logFilename+" - "+xcp.Message);
            }
        }

        private void BtnTestCredential_Click(object sender, EventArgs e)
        {
            WriteLog("Functionalitate dezactivata!!", 3);
            return;
            //lblServerInfo.Text.Replace("Initial Catalog=master", "Initial Catalog =" + apps.Where(a => a.AppName == lblAplicatie.Text).First().DBName))
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(lblServerInfo.Text);
            builder.IntegratedSecurity = false;
            var connStr = string.Format("Data Source={0};Initial Catalog={1};Integrated Security=false;User Id={2};Password={3};", builder.DataSource, builder.InitialCatalog, txtLogin.Text, txtPassword.Text);

            WriteLog("Connecting to: " + connStr);
            try
            {

                using (var connection = new SqlConnection(connStr))
                {

                    connection.Open();
                    connection.Close();
                    connection.Dispose();

                    WriteLog("Connection successful!" + connection.State, 1);

                }

            }
            catch (Exception xcp)
            {
                WriteLog("Connection failed!\n" + xcp.Message, 3);
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            ReloadPeople(txtSearch.Text);
        }

        private void BtnViewLog_Click(object sender, EventArgs e)
        {
            Process.Start(logFilename);
        }

        private void TextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ReloadPeople(txtSearch.Text);
            }
        }




        private void BtnRevokeRole_Click(object sender, EventArgs e)
        {
            var qRevokeRole = string.Format(revokeUserRole, txtLogin.Text, lblAplicatie.Text, lblTipAcces.Text);
            if (MessageBox.Show(string.Format("Rolul va fi marcat ca revocat!\nPentru a sterge si permisiunile asociate, folositi optiunile corespunzatoare!\nSigur doriti sa continuati?\n\n{0}", qRevokeRole), "Revoke Role?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                WriteLog("Stergere rol din admin aplic!");
                WriteLog(qRevokeRole, 1);
                using (var connection = new SqlConnection(adminAplicConnectionString))
                {
                    var command = new SqlCommand(qRevokeRole, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        ReloadRoles(txtLogin.Text);
                        WriteLog("OK!", 2);
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
