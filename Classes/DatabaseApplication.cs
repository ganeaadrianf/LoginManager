using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoginManager.Classes
{


    public class AccesAplicatie
    {
        const string parseErrorMessage = "Configurarea AccesAplicatieList nu pare sa fie corecta";
        public string Acces { get; set; }
        public string ConnectionString { get; set; }
        public object ConnectionInfo
        {
            get
            {
                return new List<string> { DbName, Server };
            }
        }

        public AccesAplicatie Self
        {
            get
            {
                return this;
            }
        }
        public string AppName
        {
            get
            {
                if (Acces==string.Empty)
                {
                    return string.Empty;
                }
                string value = string.Empty;
                try { value = Acces.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0]; }
                catch (Exception)
                {
                    MessageBox.Show(parseErrorMessage, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
                return value;
            }
        }
        public string AccessType
        {
            get
            {
                if (Acces == string.Empty)
                {
                    return string.Empty;
                }
                string value = string.Empty;
                try { value = Acces.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[1]; }
                catch (Exception)
                {
                    MessageBox.Show(parseErrorMessage, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
                return value;
            }
        }
        public string DbRole
        {
            get
            {
                if (Acces == string.Empty)
                {
                    return string.Empty;
                }
                string value = string.Empty;
                try { value = Acces.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[2]; }
                catch (Exception)
                {
                    MessageBox.Show(parseErrorMessage, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
                return value;
            }
        }
        public string DbName
        {
            get
            {
                if (Acces == string.Empty)
                {
                    return string.Empty;
                }
                string value = string.Empty;
                try { value = Acces.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[3]; }
                catch (Exception)
                {
                    MessageBox.Show(parseErrorMessage, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
                return value;
            }
        }
        public string Server
        {
            get
            {
                if (Acces == string.Empty)
                {
                    return string.Empty;
                }
                string value = string.Empty;
                try { value = Acces.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[4]; }
                catch (Exception)
                {
                    MessageBox.Show(parseErrorMessage, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
                return value;
            }
        }
    }
}
