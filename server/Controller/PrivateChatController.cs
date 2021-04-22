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
        private int portnum;
        private TcpListener server;
        private CHATTPYE type;

        public PrivateChatController(int port, CHATTPYE t)
        {
            count = 0;
            portnum = port;
            server = new TcpListener(IPAddress.Any, portnum);
            clients = new ConcurrentDictionary<int, TcpClient>();
            type = t;
        }


        

        public void handleConnecting()
        {
            try
            {
                server.Start();
                Thread t = new Thread(handleMessaging);
                t.Start();

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("A private chat has started!");
                    lock (llock)
                    {
                        bool success = clients.TryAdd(count, client);
                        if (!success)
                        {
                            Console.WriteLine("something shit happened");
                        }
                    }

                    count++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private void handleMessaging()
        {
            while (true)
            {
                lock (llock)
                {
                    foreach (KeyValuePair<int, TcpClient> id_client in clients)
                    {
                        int parentId = id_client.Key;
                        TcpClient client = id_client.Value;
                        NetworkStream ns = client.GetStream();
                        if (ns.DataAvailable)
                        {
                            NetworkStream stream = client.GetStream();


                            string message = Utility.ReadFromNetworkStream(stream);
                            Console.WriteLine(message);

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
                                        Console.WriteLine("writing...");
                                        channel.Write(data, 0, data.Length);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Exception during private chat on port " + portnum + "error message: " + e.Message + ". Client removed from clients");
                                        clients.Remove(id, out destination);
                                        count--;


                                        ///send out that smbd has disconnected
                                        if (type == CHATTPYE.PRIVATE)
                                        {
                                            PortManager.instance().ReturnPrivateChatPort(portnum);
                                            foreach (KeyValuePair<int, TcpClient> id_lastOne in clients)
                                            {
                                                string disconnect_msg = "Your partner has disconnected!";
                                                byte[] disconnect_data = Encoding.Unicode.GetBytes(disconnect_msg);


                                                NetworkStream clientstream = id_lastOne.Value.GetStream();
                                                clientstream.Write(disconnect_data, 0, disconnect_data.Length);
                                            }
                                            return;
                                        }
                                        else if (type == CHATTPYE.GROUP)
                                        {
                                            foreach (KeyValuePair<int, TcpClient> id_lastOne in clients)
                                            {
                                                string disconnect_msg = "Your partner has disconnected!";
                                                byte[] disconnect_data = Encoding.Unicode.GetBytes(disconnect_msg);


                                                NetworkStream clientstream = id_lastOne.Value.GetStream();
                                                clientstream.Write(disconnect_data, 0, disconnect_data.Length);
                                            }
                                        }
                                    }
                                }
                            }                                                  
                        }
                    }
                }

                Thread.Sleep(100);
            }
        }

        private void handleCommands(string command)
        {
            string[] commandargs = command.Split("|");
            if(commandargs[0] == "!LEAVE")
            {
                foreach(KeyValuePair<int, TcpClient> id_destination in clients)
                {
                    TcpClient destination = id_destination.Value;
                    NetworkStream stream = destination.GetStream();
                    string disconnect_msg = commandargs[1] + " has disconnected!";
                    byte[] disconnect_data = Encoding.Unicode.GetBytes(disconnect_msg);
                    stream.Write(disconnect_data, 0, disconnect_data.Length);
                }
            }
            else
            {
                Console.WriteLine("Unknown command!");
            }
        }
    }
}
