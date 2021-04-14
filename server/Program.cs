using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using server.Controller;
using System.Threading;

namespace server
{
    class Program
    {
        static void Main(string[] args)
        {
            LoginController lc = LoginController.instance();
            DatabaseController dc = DatabaseController.instance();
            MatchController mc = MatchController.instance();

            Thread login = new Thread(lc.logincontrol);
            login.Start();

            Thread match = new Thread(mc.handleRequests);

            //dc.successfulRegister("admin", "admin".GetHashCode().ToString(), 1, 1);

        }
    }
}
