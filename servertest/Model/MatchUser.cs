using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Data;
using System.Text;
using System.Collections.Concurrent;
namespace servertest.Model
{
    class MatchUser
    {

        private int id;
        private string username;
        private AGECATEGORY age;
        private GENDER sex;
        private GENDER lookingforSex; ///ngl its kinda funny
        private TcpClient client;

        #region Properties
        public int Id { get { return id; } set { id = value; } }
        public string Username { get { return username; } set { username = value; } }
        public AGECATEGORY Age { get { return age; } set { age = value; } }
        public GENDER Sex { get { return sex; } set { sex = value; } }
        public GENDER LookingForSex { get { return lookingforSex; } set { lookingforSex = value; } }
        public TcpClient Client { get { return client; } set { client = value; } }
        #endregion

        public bool archEqual(MatchUser candidate)
        {
            bool success = candidate.age == this.age && candidate.lookingforSex == this.lookingforSex;


            return success;
        }

        public MatchUser() { }

        public MatchUser(int id)
        {
            this.id = id;
        }

        /// <summary>
        /// for archtype uses ONLY!
        /// </summary>
        /// <param name="archuser"></param>
        public MatchUser(MatchUser archuser)
        {
            this.age = archuser.age;
            this.lookingforSex = archuser.lookingforSex;
            id = -1;
            username = "";
            client = null;
        }

        public void setArch(GENDER sex, AGECATEGORY age)
        {
            this.age = age;
            this.lookingforSex = sex;
            id = -1;
            username = "";
            client = null;
        }

        public MatchUser getArch()
        {
            MatchUser archuser = new MatchUser(this);
            return archuser;
        }

        public override string ToString()
        {
            return username + " " + sex.ToString() + " " + age.ToString() + " " + lookingforSex.ToString();
        }
    }
}
