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
            DatabaseController dc = DatabaseController.instance();
            LoginController lc = LoginController.instance();
            MatchController mc = MatchController.instance();

            Thread login = new Thread(lc.logincontrol);
            login.Start();

            Thread match = new Thread(mc.handleRequests);
            match.Start();

            //string res = dc.GetAgeAndGender("theVilandry");
            //Console.WriteLine(res);
            

            //dc.successfulRegister("admin", "admin".GetHashCode().ToString(), 1, 1);

        }
    }
}
