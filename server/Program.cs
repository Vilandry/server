using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using server.Controllers;
using System.Threading;

namespace server
{
    class Program
    {
        SqlConnection conn;
        static void Main(string[] args)
        {
            LoginController lc = new LoginController();
            DatabaseController dc = DatabaseController.instance();

            Thread t = new Thread(new ThreadStart(lc.logincontrol));
            t.Start();

            //dc.successfulRegister("admin", "admin".GetHashCode().ToString(), 1, 1);

        }
    }
}
