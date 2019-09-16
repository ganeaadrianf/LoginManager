using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginManager.Classes
{
    public class DatabaseApplication
    {

        public string AppName { get; set; }
        public string DBName { get; set; }

        public object ConnectionInfo
        {
            get
            {
                return  new List<string> { DBName, DBServer };
            }
        }

        public string DBServer { get; set; }

        public string ConnectionString { get; set; }

        public List<string> DBRoles { get; set; }


    }
}
