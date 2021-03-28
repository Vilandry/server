using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;

namespace server.Model
{
    class SUser
    {
        private int id;
        private string username;
        private TcpClient client;

        public int Id { get { return id; } }
        public string Username { get { return username; } }
        public TcpClient Client { get { return client; } set { client = value; } }
    }
}
