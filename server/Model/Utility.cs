using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace server.Model
{
    public static class Utility
    {



        public static string ReadFromNetworkStream(NetworkStream stream)
        {
            byte[] bytes = new Byte[256];
            string message = null;
            int i = 0, byteCount = 0;
            do
            {
                i = stream.Read(bytes, 0, bytes.Length);
                Console.WriteLine(i);
                // Translate data bytes to a ASCII string.
                message = System.Text.Encoding.ASCII.GetString(bytes, byteCount, i);
                byteCount += i;
            } while (stream.DataAvailable);

            return message;
        }
    }
}
