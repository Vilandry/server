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
using server.Controller;

namespace server.Controller
{
    class MiscController
    {
        private static readonly object llock = new object();
        TcpListener server;

        private static MiscController inst;

        private MiscController() { }

        public static MiscController instance()
        {
            if(inst == null)
            {
                inst = new MiscController();
            }
            return inst;
        }

        public void handleRequests()
        {
            server = new TcpListener(IPAddress.Any, PortManager.instance().Miscport);
            server.Start();

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
                string inserter = commandargs[2];
                bool wasSaved = DatabaseController.instance().alreadySavedChatHistory(savename);

                if(wasSaved)
                {
                    try
                    {
                        DatabaseController.instance().InsertMessageHistoryConnection(savename, inserter);
                        Console.WriteLine("MiscController: inserted message history for " + inserter + " as " + savename);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("MiscController error: could not reach client, error message: " + e.Message);
                        return;
                    }


                    try
                    {
                        byte[] okmsg = Encoding.Unicode.GetBytes("OK");
                        stream.Write(okmsg);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("MiscController error: could not reach client, error message: " + e.Message);
                        return;
                    }
                    
                }
                else
                {
                    try
                    {
                        byte[] okmsg = Encoding.Unicode.GetBytes("INSERT");
                        stream.Write(okmsg);


                        Thread.Sleep(100);
                        //Console.WriteLine("reading history...");
                        string history = Utility.ReadFromNetworkStream(stream);

                        

                        if(DatabaseController.instance().InsertMessageHistoryConnection(savename, inserter))
                        {
                            if(DatabaseController.instance().InsertMessageHistoryText(savename, history))
                            {
                                okmsg = Encoding.Unicode.GetBytes("OK");
                                stream.Write(okmsg);
                                Console.WriteLine("MiscController: inserted message history for " + inserter + " as " + savename + " with text!");
                            }
                            else
                            {
                                okmsg = Encoding.Unicode.GetBytes("ER");
                                Console.WriteLine("MiscController notice: could not insert history text!");
                                stream.Write(okmsg);
                            }
                        }
                        else
                        {
                            Console.WriteLine("MiscController notice: could not insert history connection!");
                            okmsg = Encoding.Unicode.GetBytes("ER");
                            stream.Write(okmsg);
                        }
                    
                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("MiscController notice: could not reach client after inserting history text, exception message: " + e.Message);
                        return;
                    }
                }
            }
        }
    }
}
