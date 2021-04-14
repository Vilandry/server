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

using server.Model;

namespace server.Controller
{
    class MatchController
    {
        private static readonly object llock = new object();
        private List<MatchUser> clients;
        private Dictionary<MatchUser, bool> cantMatch;
        private TcpListener server;
        private static MatchController inst;
        private int count;


        public static MatchController instance()
        {
            if(inst == null)
            {
                inst = new MatchController();
            }

            return inst;
        }

        private MatchController() { count = 0; }




        /// <summary>
        /// Handles the incoming chat requests
        /// </summary>
        public void handleRequests()
        {
            server = new TcpListener(IPAddress.Any, 9900);
            clients = new List<MatchUser>();

            server.Start();


            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                MatchUser joineduser = RecreateUser(client);

                Console.WriteLine("Joined " + joineduser.ToString());

                clients.Add(joineduser);
                handleMatches();
            }
        }

        void handleMatches()
        {
            cantMatch = new Dictionary<MatchUser, bool>(); ///in this, we will store the known "unmatchable" users

            for (int i = 0; i < clients.Count; i++)
            {
                MatchUser curUser = clients[i];
                for (int j = i + 1; j < clients.Count; j++)
                {
                    MatchUser candidate = clients[j];


                    bool success = lookingForThem(curUser, candidate);
                    if (success)
                    {
                        ///create a new instance of privatechat-controller, and match them
                        Console.WriteLine("ITS A MATCH! Matched " + curUser.Username + " WITH " + candidate.Username + "!");

                        PrivateChatController pcc = new PrivateChatController();
                    }
                }
            }
        }

        /// <summary>
        /// reads fron the client's stream and returns a serveruser with the readed data
        /// </summary>
        /// <param name="client">The client which will provide the data for the user</param>
        /// <returns></returns>
        MatchUser RecreateUser(TcpClient client)
        {
            MatchUser queued = null;
            try
            {
                queued = new MatchUser();
                queued.Client = client;
                NetworkStream ns = client.GetStream();

                int buffersize = 1024;
                byte[] data = new byte[1024];
                ns.Read(data, 0, buffersize);

                string raw_info = System.Text.Encoding.UTF8.GetString(data);
                ///FORMAT: Username|age|sex|lookingforsex

                string[] info = raw_info.Split("|");

                queued.Username = info[0];
                queued.Age = (AGECATEGORY)Enum.Parse(typeof(AGECATEGORY), info[1]);
                queued.Sex = (GENDER)Enum.Parse(typeof(GENDER), info[2]);
                queued.LookingForSex = (GENDER)Enum.Parse(typeof(GENDER), info[3]);


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }



            return queued;
        }


        /// <summary>
        /// Determines if the curUser is looking for the candidate. If not, it will be added to the cantMatch set
        /// </summary>
        /// <param name="curUser"></param>
        /// <param name="candidate"></param>
        /// <returns></returns>
        bool lookingForThem(MatchUser curUser, MatchUser candidate)
        {
            bool success = (candidate.Age == curUser.Age); ///first step

            success = success && (candidate.Sex == curUser.LookingForSex || curUser.LookingForSex == GENDER.ANY); ///we are cool if the candidate is in the gender we are looking for OR if we dont care about it at all
            success = success && (curUser.Sex == candidate.LookingForSex || candidate.LookingForSex == GENDER.ANY); ///and vice versa

            if (success == false) ///then add it to the "unmatchable" group.
            {
                cantMatch.Add(new MatchUser(curUser), false);

                if (curUser.LookingForSex == GENDER.ANY) ///if it was that flexible, add all gender into the unmatchable group
                {
                    foreach (GENDER sex in Enum.GetValues(typeof(GENDER)))
                    {
                        MatchUser archuser = new MatchUser();
                        archuser.setArch(sex, curUser.Age);
                        cantMatch.Add(archuser, false);
                    }
                }
            }
            return success;
        }

    }
}
