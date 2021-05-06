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

using server.Model;


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
            TcpListener server = new TcpListener(IPAddress.Any, PortManager.instance().Loginport);
            server.Start();


            while (true)
            {
                // Buffer for reading data
                byte[] bytes = new Byte[256];
                string message = null;
                TcpClient client = new TcpClient();

                try
                {
                    client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();

                    KeyValuePair<bool, string> pair = Utility.ReadFromNetworkStream(stream);

                    if(pair.Key == false) { Console.WriteLine("LoginController: invalid syntax on message, discarding request."); continue; }

                    message = pair.Value;

                    Console.WriteLine("LoginController:  recieved during login or register attempt: " + message);


                    ///<<------------------->>///

                    /*int buffersize = 256;
                    byte[] data = new byte[buffersize];
                    stream.Read(data, 0, buffersize);
                    message = System.Text.Encoding.ASCII.GetString(data);
                    Console.WriteLine("Recieved during login: " + message);*/


                    // Process the data sent by the client.
                    string[] raw_text = message.Split("|");
                    bool success = false;
                    string username = "";

                    if (raw_text[0] == "LOGIN")
                    {
                        username = raw_text[1];
                        string password = raw_text[2];

                        success = DatabaseController.instance().successfulLogin(username, password);
                    }
                    else if (raw_text[0] == "REGISTER")
                    {
                        username = raw_text[1];
                        string password = raw_text[2];
                        int age = Int32.Parse(raw_text[3]);
                        int sex = Int32.Parse(raw_text[4]);

                        success = DatabaseController.instance().successfulRegister(username, password, age, sex);
                    }///more features to be added if needed

                    byte[] msg;
                    string log;
                    if (success)
                    {                        
                        log = "OK|";
                        log = log + DatabaseController.instance().GetAgeAndGender(username);
                        msg = Encoding.Unicode.GetBytes(log);

                        Console.WriteLine("LoginController: Successful login, login data: " + log);
                    }
                    else
                    {                     
                        log = "ER|-1|-1";
                        msg = Encoding.Unicode.GetBytes(log);
                    }


                    // Send back a response.
                    stream.Write(msg, 0, msg.Length);
                    Console.WriteLine("LoginController: " + log + "data was sent to the client!");

                    
                }
                catch(Exception e)
                {
                    Console.WriteLine("LoginController exception: probably someone left during login attempt, error message: " + e.Message);
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
