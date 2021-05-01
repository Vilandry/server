using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using server.Model;

namespace server.Controller
{
    class PrivateChatController
    {
        private static readonly object llock = new object();
        private static ConcurrentDictionary<int, TcpClient> clients;
        private int count;
        private int id;
        private int portnum;
        private TcpListener server;
        private CHATTPYE type;
        private bool ongoing;

        public PrivateChatController(int port, CHATTPYE t)
        {
            count = 0;
            id = 0;
            portnum = port;
            server = new TcpListener(IPAddress.Any, portnum);
            clients = new ConcurrentDictionary<int, TcpClient>();
            type = t;
            ongoing = true;
        }


        

        public void handleConnecting()
        {
            try
            {
                server.Start();
                Thread t = new Thread(handleMessaging);
                t.Start();

                while (ongoing)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("A client has joined to the private chat on port " + portnum);
                    lock (llock)
                    {
                        bool success = clients.TryAdd(id, client);
                        if (!success)
                        {
                            Console.WriteLine("Couldnt add the joining client to the clientlist on port " + portnum);
                        }
                    }

                    count++;
                    id++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error on port " + portnum + ", error message: " + e.Message);
            }

        }

        private void handleMessaging()
        {
            while (ongoing)
            {
                lock (llock)
                {
                    foreach (KeyValuePair<int, TcpClient> id_client in clients)
                    {
                        int parentId = id_client.Key;
                        TcpClient client = id_client.Value;
                        NetworkStream ns = client.GetStream();
                        Console.WriteLine("Trying to get data on port " + portnum);
                        Console.WriteLine("Trying to read on port " + portnum + "with result of " + ns.CanRead + " and dataavailable: " + ns.DataAvailable);

                        if (ns.DataAvailable)
                        {
                            Console.WriteLine("Dataavailable on portnum " + portnum);

                            string message = Utility.ReadFromNetworkStream(ns);
                            Console.WriteLine(message);
                            //conversationHistory.Add(Utility.EscapePrivateChat(message));

                            if (message[0] == '!')
                            {
                                handleCommands(message);
                            }
                            else
                            {
                                foreach (KeyValuePair<int, TcpClient> id_destination in clients)
                                {
                                    int id = id_destination.Key;
                                    //if (id == parentId) { continue; }

                                    TcpClient destination = id_destination.Value;
                                    NetworkStream channel = destination.GetStream();
                                    try
                                    {
                                        byte[] data = Encoding.Unicode.GetBytes(message);
                                        channel.Write(data, 0, data.Length);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Exception during private chat on port " + portnum + "error message: " + e.Message + ". Client removed from clients");
                                        RemoveDeadClient(id, destination);


                                        ///send out that smbd has disconnected
                                        if (type == CHATTPYE.PRIVATE)
                                        {
                                            PortManager.instance().ReturnPrivateChatPort(portnum);
                                            ongoing = false;
                                            return;
                                        }
                                        else if (type == CHATTPYE.GROUP)
                                        {
                                            if(count==0)
                                            {
                                                PortManager.instance().ReturnGroupChatPort(portnum);
                                                ongoing = false;
                                                return;
                                            }
                                        }
                                    }
                                }
                            }                                                  
                        }
                    }
                }

                Thread.Sleep(5000);
                Console.WriteLine("Privatechat on port " + portnum + "is alive! Number of participants: " + clients.Count);
                //Thread.Yield();
            }
        }

        private void RemoveDeadClient(int id, TcpClient deadclient)
        {
            clients.Remove(id, out deadclient);
            count--;
            Console.WriteLine("Dead client removed with id: " + id + "on port " + portnum);
            foreach (KeyValuePair<int, TcpClient> id_lastOne in clients)
            {

                string disconnect_msg = "SERVER|!LEFT|Your partner";
                byte[] disconnect_data = Encoding.Unicode.GetBytes(disconnect_msg);

                try
                {
                    NetworkStream clientstream = id_lastOne.Value.GetStream();
                    clientstream.Write(disconnect_data, 0, disconnect_data.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message + ", removing the dead client");
                    RemoveDeadClient(id_lastOne.Key, id_lastOne.Value);
                }

            }

            if(type == CHATTPYE.PRIVATE)
            {
                ongoing = false;
            }
        }

        private void handleCommands(string command)
        {
            string[] commandargs = command.Split("|");
            Console.WriteLine("HandleCommand: " + command + " on portnum " + portnum);
            if(commandargs[0] == "!LEAVE")
            {
                foreach(KeyValuePair<int, TcpClient> id_destination in clients)
                {
                    TcpClient destination = id_destination.Value;
                    NetworkStream stream = destination.GetStream();
                    string disconnect_msg = "SERVER|" + "!LEFT|" + commandargs[1];
                    byte[] disconnect_data = Encoding.Unicode.GetBytes(disconnect_msg);
                    stream.Write(disconnect_data, 0, disconnect_data.Length);
                    
                    Console.WriteLine("UPortManager: ending chat on port " + portnum);
                }
                ongoing = false;
            }
            else
            {
                Console.WriteLine("Unknown command arrived on portnum " + portnum);
            }
        }
    }
}
