/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace server.Model
{
    public class PrivateChatMatch
    {
        private SUser user1;
        private SUser user2;
        private TcpListener server;

        private List<TcpClient> listOfClients; ///using this model since it will allow us later to reuse the model for group chatting


        public PrivateChatMatch(SUser u1, SUser u2, int portnum)
        {
            user1 = u1;
            user2 = u2;
            server = new TcpListener(IPAddress.Any, portnum);
            server.Start();
        }

        void handleCommand(string commandString)
        {

        }

        public void HandleChat()
        {
            while (true)
            {
                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;
                TcpClient client = new TcpClient();

                try
                {
                    user1.Client = server.AcceptTcpClient();

                    Console.WriteLine("here we go");

                    NetworkStream stream = client.GetStream();

                    int i, byteCount = 0;

                    // Loop to receive all the data sent by the client.
                    do
                    {
                        //Console.WriteLine("here");
                        i = stream.Read(bytes, 0, bytes.Length);
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, byteCount, i);
                        byteCount += i;
                        Console.WriteLine("Received: {0}", data);
                        Console.WriteLine(i.ToString());


                    } while (i >= bytes.Length); ///this means, we read less than we could have, so we read all


                    // Process the data sent by the client.
                    string[] raw_text = data.Split("|");
                    bool success = false;

                    
                    byte[] msg;
                    string log;
                    if (success)
                    {
                        msg = System.Text.Encoding.ASCII.GetBytes("OK");
                        log = "OK";
                    }
                    else
                    {
                        msg = System.Text.Encoding.ASCII.GetBytes("ER");
                        log = "ER";
                    }


                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    stream.Close();
                    Console.WriteLine("Sent: {0}", log);


                }
                catch (Exception e)
                {
                    Console.WriteLine("probably someone left");
                }
                finally
                {
                    // Shutdown and end connection
                    client.Close();
                }
            }
        }
    }
}
*/