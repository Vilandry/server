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


        public static MatchController instance()
        {
            if(inst == null)
            {
                inst = new MatchController();
            }

            return inst;
        }

        private MatchController() { }




        /// <summary>
        /// Handles the incoming chat requests
        /// </summary>
        public void handleRequests()
        {
            server = new TcpListener(IPAddress.Any, PortManager.instance().Matchport);
            clients = new List<MatchUser>();

            server.Start();


            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream ns = client.GetStream();

                try
                {
                    int buffersize = 1024;
                    byte[] data = new byte[1024];
                    ns.Read(data, 0, buffersize);


                    string raw_info = System.Text.Encoding.UTF8.GetString(data);

                    MatchUser joineduser = RecreateUser(raw_info);
                    joineduser.Client = client;

                    Console.WriteLine("Joined " + joineduser.ToString());

                    byte[] okmsg = System.Text.Encoding.ASCII.GetBytes("ok");
                    ns.Write(okmsg, 0, okmsg.Length);

                    clients.Add(joineduser);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Exception in MatchManaging, error message: " + e.Message);
                }

                handleMatches();
            }
        }


        private void handleMatches()
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



                        int port = PortManager.instance().GetPrivateChatPort();
                        if(port == -1)
                        {
                            Console.WriteLine("No available port, no match will happen! Consider restarting the server with a wilder area of ports.");
                            continue;
                        }

                        string portString = "" + port;
                        Byte[] portdata = System.Text.Encoding.ASCII.GetBytes(portString);

                        NetworkStream portstream = curUser.Client.GetStream();
                        portstream.Write(portdata, 0, portdata.Length);

                        portstream = candidate.Client.GetStream();
                        portstream.Write(portdata, 0, portdata.Length);


                        PrivateChatController pcc = new PrivateChatController(port, CHATTPYE.PRIVATE);
                        Thread privateChatThread = new Thread(pcc.handleConnecting);

                        privateChatThread.Start();

                        clients.RemoveAt(i); ///ok tbh it kinda looks scray
                        clients.RemoveAt(j);
                        i--;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// reads fron the client's stream and returns a serveruser with the readed data
        /// </summary>
        /// <param name="raw_info">The raw data used for recreate the user</param>
        /// <returns></returns>
        private MatchUser RecreateUser(string raw_info)
        {
            MatchUser queued = null;
            try
            {
                queued = new MatchUser();
                
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
        private bool lookingForThem(MatchUser curUser, MatchUser candidate)
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
