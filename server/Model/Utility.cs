using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace server.Model
{
    public static class Utility
    {

        public static string EscapePrivateChat(string history)
        {
            history.Replace("<e>", "<e><e>"); ///<e> is escape, <f> is newline
            history.Replace("<f>", "<e><f>");
            history.Replace("\n", "<f>");


            return history;
        }

        public static bool passwordEquals(string ogpassword, string dbpassword)
        {
            bool success = true;
            for(int i=0; i<Math.Min(ogpassword.Length,dbpassword.Length); i++)
            {
                success = success && (ogpassword[i] == dbpassword[i]);
            }
            return success;
        }

        public static KeyValuePair<bool, string> Validate(string message)
        {
            bool success = false;
            string text = "";

            string[] components = message.Split("|",2);
            if(components.Length == 1 )
            {
                success = false;
            }
            else
            {
                if(components[0] != "KNOCKNOCK")
                {
                    success = false;
                }
                else
                {
                    success = true;
                    text = components[1];
                }    
            }


            return new KeyValuePair<bool, string>(success, text);
        }


        public static KeyValuePair<bool, string> ReadFromNetworkStream(NetworkStream stream)
        {
            byte[] bytes;
            string message = "";
            int i = 0, byteCount = 0;
            do
            {
                Thread.Sleep(50);
                bytes = new Byte[1024];
                i = stream.Read(bytes, 0, bytes.Length);
                // Translate data bytes to a ASCII string.
                //message = Encoding.Unicode.GetString(bytes, byteCount, i);
                string newmessage = Encoding.Unicode.GetString(bytes, 0, i);
                message = message + newmessage;
                byteCount += i;
                //Console.WriteLine(newmessage);
                
            } while (stream.DataAvailable);



                return Validate(message);
        }
    }
}
