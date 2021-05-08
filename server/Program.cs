using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using server.Controller;
using System.Threading;
using server.Model;

namespace server
{
    class Program
    {
        private static string portconfigpath;
        private static string databaseconfigpath;
        static void Main(string[] args)
        {
            try
            {
                databaseconfigpath = args[0];
                Console.WriteLine("Using databaseconfigfile " + args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine("No databaseconfigpath was given. Exiting..." + e.Message);
                databaseconfigpath = "";
            }

            try
            {
                portconfigpath = args[1];
                Console.WriteLine("Using portconfigfile " + args[1]);
            }
            catch(Exception e)
            {
                Console.WriteLine("No portconfigpath was given, using default ports. (exception message: " + e.Message + ")");
                portconfigpath = "";
            }

            PortManager.instance();
            Thread.Sleep(1000);
            //Console.WriteLine("here");


            DatabaseController dc = DatabaseController.instance();
            Thread.Sleep(1000);
            LoginController lc = LoginController.instance();
            Thread.Sleep(100);
            MatchController mc = MatchController.instance();
            Thread.Sleep(100);
            MiscController miscc = MiscController.instance();
            Thread.Sleep(100);

            Thread login = new Thread(lc.logincontrol);
            login.Start();

            Thread match = new Thread(mc.handleRequests);
            match.Start();

            Thread misc = new Thread(miscc.handleRequests);
            misc.Start();

            //string res = dc.GetAgeAndGender("theVilandry");
            //Console.WriteLine(res);
            

            //dc.successfulRegister("admin", "admin".GetHashCode().ToString(), 1, 1);

        }
        public static string Portconfigpath { get{ return portconfigpath; } }
        public static string Databaseconfigpath { get { return databaseconfigpath; } }

    }
}
