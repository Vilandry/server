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
                    Console.WriteLine("smbd arrived!");
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
                            byte[] buffer = new byte[1024]; ///this should be sufficient
                            int byte_count = stream.Read(buffer, 0, buffer.Length);

                            string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                            Console.WriteLine(data);

                            foreach (KeyValuePair<int, TcpClient> id_destination in clients)
                            {
                                int id = id_destination.Key;
                                //if (id == parentId) { continue; }

                                TcpClient destination = id_destination.Value;
                                NetworkStream channel = destination.GetStream();
                                try
                                {
                                    channel.Write(buffer, 0, buffer.Length);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                    Console.WriteLine("prob smbd disconnected");
                                    clients.Remove(id, out destination);
                                    count--;

                                    if(type == CHATTPYE.PRIVATE)
                                    {
                                        ///send out 
                                        foreach (KeyValuePair<int, TcpClient> id_lastOne in clients)
                                        {

                                        }
                                        ///send event
                                    }
                                }

                            }
                        }
                    }
                }

                Thread.Sleep(100);
            }
        }
    }
}
