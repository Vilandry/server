/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using server.Model;

namespace server.Model
{
    public class MatchUser
    {
        private int id;
        private string username;
        private TcpClient client;
        private GENDER sex;
        private AGECATEGORY age;

        public int Id { get { return id; } }
        public string Username { get { return username; } }
        public TcpClient Client { get { return client; } set { client = value; } }
        public GENDER Gender { get { return sex; } set { sex = value; } }
        public AGECATEGORY Age { get { return age; } set { age = value; } }
    }
}
*/