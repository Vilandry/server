using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;


namespace server.Controller
{
    class LoginController
    {
        private static LoginController lc;

        public static LoginController instance()
        {
            if(lc==null)
            {
                lc = new LoginController();
            }
            return lc;
        }

        private LoginController() { }
        public void logincontrol()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 11000);
            server.Start();


            while (true)
            {
                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;
                TcpClient client = new TcpClient();

                try
                {
                    client = server.AcceptTcpClient();

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

                    if (raw_text[0] == "LOGIN")
                    {
                        string username = raw_text[1];
                        Console.WriteLine(raw_text[2]);
                        string password = raw_text[2];

                        success = DatabaseController.instance().successfulLogin(username, password);
                    }
                    else if (raw_text[0] == "REGISTER")
                    {
                        string username = raw_text[1];
                        string password = raw_text[2];
                        int age = Int32.Parse(raw_text[3]);
                        int sex = Int32.Parse(raw_text[4]);

                        success = DatabaseController.instance().successfulRegister(username, password, age, sex);
                    }///more features to be added if needed

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
                catch(Exception e)
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
