using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Data;
using System.Text;
using System.Collections.Concurrent;

using server.Model;

namespace server.Controller
{
    class MiscController
    {
        private static readonly object llock = new object();
        TcpListener server;

        private static MiscController misc;

        private MiscController() { }

        public MiscController inst()
        {
            if(misc == null)
            {
                misc = new MiscController();
            }
            return misc;
        }

        public void handleRequests()
        {
            server = new TcpListener(IPAddress.Any, PortManager.instance().Miscport);

            while(true)
            {
                TcpClient client = server.AcceptTcpClient();

                try
                {
                    NetworkStream stream = client.GetStream();
                    string command = Utility.ReadFromNetworkStream(stream);
                    handleCommands(command, stream);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Error in MiscManager, error message: " + e.Message);
                }
            }
        }

        private void handleCommands(string command, NetworkStream stream)
        {
            string[] commandargs = command.Split("|");

            if(commandargs[0] == "CONVSAVE")
            {
                string savename = commandargs[1];
                bool wasSaved = DatabaseController.instance().alreadySavedChatHistory(savename);

                if(wasSaved)
                {
                    try
                    {
                        byte[] okmsg = Encoding.Unicode.GetBytes("OK");
                        stream.Write(okmsg);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("MiscController error: could not reach client, error message" + e.Message);
                        return;
                    }
                    
                }
                else
                {

                }
            }
        }
    }
}
