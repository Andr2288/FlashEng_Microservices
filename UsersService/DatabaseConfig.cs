using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersService
{
    public static class DatabaseConfig
    {

        public static string ConnectionString =>
            "Server=localhost;Database=flasheng_users;User=admin;Password=1234567890;";
    }
}
