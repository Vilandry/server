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
using System.Diagnostics;

using servertest.Model;

namespace servertest.Controller
{
    class MatchController
    {
        private static readonly object llock = new object();
        private List<MatchUser> clients;
        //private Dictionary<MatchUser, bool> cantMatch;
        private TcpListener server;
        private static MatchController inst;
        private ConcurrentDictionary<PrivateChatController, Thread> privatechatsThreads;

        public static MatchController instance()
        {
            if (inst == null)
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
            privatechatsThreads = new ConcurrentDictionary<PrivateChatController, Thread>();

            server.Start();

            Thread commandThread = new Thread(handleInputCommands);
            commandThread.Start();

            Thread managerThread = new Thread(ThreadManaging);
            managerThread.Start();

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream ns = client.GetStream();

                try
                {
                    KeyValuePair<bool, string> pair = Utility.ReadFromNetworkStream(ns);

                    if (pair.Key == false) { Console.WriteLine("MatchController: invalid syntax on message, discarding request."); continue; }

                    string raw_info = pair.Value;

                    //string raw_info = Utility.ReadFromNetworkStream(ns);
                    //Console.WriteLine("MatchController: Raw info: " + raw_info);

                    if (raw_info[0] == '!')
                    {
                        handleCommands(raw_info);
                    }
                    else
                    {
                        MatchUser joineduser = RecreateUser(raw_info);
                        joineduser.Client = client;

                        if(joineduser.Username == "") { continue; }

                        if(alreadyInQueue(joineduser.Username))
                        {
                            Console.WriteLine("MatchController: " + joineduser.Username + "is already in the matchqueue, new request is discarded!");

                            byte[] erdata = Encoding.Unicode.GetBytes("ER|INQUEUE");

                            ns.Write(erdata, 0, erdata.Length);
                            continue;
                        }

                        Trace.WriteLine("here");

                        Trace.WriteLine("MatchController: Joined " + joineduser.ToString());
                        Console.WriteLine("MatchController: Joined " + joineduser.ToString());

                        byte[] data = Encoding.Unicode.GetBytes("OK");

                        ns.Write(data, 0, data.Length);
                        Console.WriteLine("MatchController: Replied with " + Encoding.Unicode.GetString(data));
                        Trace.WriteLine("MatchController: Replied with " + Encoding.Unicode.GetString(data));

                        lock (llock)
                        {
                            clients.Add(joineduser);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in MatchManaging, error message: " + e.Message);
                }


                Thread.Sleep(200);
                handleMatches();
            }
        }


        private bool alreadyInQueue(string username)
        {
            bool alreadyInQueue = false;
            foreach(MatchUser user in clients)
            {
                alreadyInQueue = alreadyInQueue || (user.Username == username);
            }

            return alreadyInQueue;
        }

        private void handleMatches()
        {
            //cantMatch = new Dictionary<MatchUser, bool>(); ///in this, we will store the known "unmatchable" users

            for (int i = 0; i < clients.Count; i++)
            {
                MatchUser curUser = clients[i];
                for (int j = i + 1; j < clients.Count; j++)
                {
                    MatchUser candidate = clients[j];
                    List<MatchUser> removeables = new List<MatchUser>();

                    Trace.WriteLine(curUser.Username + "    " + candidate.Username);
                    bool success = lookingForThem(curUser, candidate);
                    Trace.WriteLine(success);
                    if (success)
                    {
                        ///create a new instance of privatechat-controller, and match them
                        Console.WriteLine("MatchController: ITS A MATCH! Matched " + curUser.Username + " WITH " + candidate.Username + "!");
                        Trace.WriteLine("MatchController: ITS A MATCH! Matched " + curUser.Username + " WITH " + candidate.Username + "!");


                        int port = PortManager.instance().GetPrivateChatPort();
                        if (port == -1)
                        {
                            Console.WriteLine("MatchController: No available port, no match will happen! Consider restarting the server with a wilder area of ports. Waiting for 1 second then continue"); Trace.WriteLine("MatchController: No available port, no match will happen! Consider restarting the server with a wilder area of ports. Waiting for 1 second then continue");
                            Thread.Sleep(10000);
                            handleMatches();
                            return;
                        }

                        string portString = "" + port;
                        byte[] portdata = Encoding.Unicode.GetBytes(portString);
                        Console.WriteLine("MatchController: The following port will be used for match: " + port);

                        NetworkStream curuserStream = curUser.Client.GetStream();
                        NetworkStream candidatestream = candidate.Client.GetStream();

                        ///First attempt to reach the client with the portdata
                        try
                        {
                            curuserStream.Write(portdata, 0, portdata.Length);
                            Console.WriteLine("MatchController: Portnum " + port + " sent to the client!");
                        }
                        catch (Exception e) ///if we lost the first one, reach out the second one, and remove the first
                        {
                            Console.WriteLine("MatchController error: Error during matchmaking: couldnt reach client, error message: " + e.Message);
                            /*clients.RemoveAt(i);
                            i--;*/
                            removeables.Add(clients[i]);
                            PortManager.instance().ReturnPrivateChatPort(port);

                            try
                            {
                                byte[] ermsg = Encoding.Unicode.GetBytes("ER");
                                candidatestream.Write(ermsg, 0, ermsg.Length);
                            }
                            catch (Exception f) ///if we lost that one too, remove it too
                            {
                                Console.WriteLine("MatchController error: Error during matchmaking: couldnt reach candidate, error message: " + f.Message);
                                //clients.RemoveAt(j - 1);
                                removeables.Add(clients[j]);
                            }

                            foreach(MatchUser removeable in removeables)
                            {
                                removeFromClientList(removeable.Username);
                            }
                            break;
                        }

                        Thread.Sleep(200); ///wait a bit to make sure, every package has arrived
                                           ///First attempt to reach the candidate with the portdata
                        try
                        {
                            candidatestream.Write(portdata, 0, portdata.Length);
                            Console.WriteLine("Portnum " + port + " sent to the candidate!");
                        }
                        catch (Exception e) ///if we lost the candidate
                        {
                            Console.WriteLine("MatchController error: Error during matchmaking: couldnt reach candidate, error message: " + e.Message);
                            //clients.RemoveAt(j);
                            removeables.Add(clients[j]);
                            PortManager.instance().ReturnPrivateChatPort(port);

                            try
                            {
                                byte[] ermsg = Encoding.Unicode.GetBytes("ER");
                                curuserStream.Write(ermsg, 0, ermsg.Length);
                            }
                            catch (Exception f) ///if we lost the original meanwhile
                            {
                                Console.WriteLine("MatchController error: Error during matchmaking: couldnt reach client, error message: " + f.Message);
                                /*clients.RemoveAt(i);
                                i--;
                                break;*/

                                removeables.Add(clients[i]);
                            }

                            foreach (MatchUser removeable in removeables)
                            {
                                removeFromClientList(removeable.Username);
                            }
                            break;
                        }


                        Thread.Sleep(200); ///wait a bit to make sure, every package has arrived
                                           ///send the next amount of data: the verifying
                                           ///start with the client


                        TimeSpan curtime = DateTime.UtcNow - new DateTime(1970, 1, 1);
                        int secondsSinceEpoch = (int)curtime.TotalSeconds;

                        string conversationid = "OK|" + secondsSinceEpoch + "|" + curUser.Username + "|" + candidate.Username;
                        Console.WriteLine("MatchController: okmessage: " + conversationid);
                        byte[] okmsg = Encoding.Unicode.GetBytes(conversationid);
                        try
                        {
                            //byte[] okmsg = Encoding.Unicode.GetBytes("OK");
                            curuserStream.Write(okmsg, 0, okmsg.Length);
                            Console.WriteLine("MatchController: Okmsg sent to the client!");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("MatchController error: Error during matchmaking: the client left during that little timestamp, error message: " + e.Message);
                        }

                        Thread.Sleep(200); ///wait a bit to make sure, every package has arrived
                                           ///then to the candidate
                                           
                        
                        try
                        {
                            //byte[] okmsg = Encoding.Unicode.GetBytes("OK");
                            candidatestream.Write(okmsg, 0, okmsg.Length);
                            Console.WriteLine("MatchController: Okmsg sent to the candidate!");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("MatchController: Error during matchmaking: the candidate left during that little timestamp, error message " + e.Message);
                        }

                        PrivateChatController pcc = new PrivateChatController(port, CHATTPYE.PRIVATE);
                        Thread privateChatThread = new Thread(pcc.handleConnecting);
                        privateChatThread.IsBackground = true;
                        privateChatThread.Start();

                        privatechatsThreads.TryAdd(pcc, privateChatThread);
                        

                        lock (llock)
                        {

                            clients.RemoveAt(i); ///ok tbh it kinda looks scray
                            clients.RemoveAt(j - 1);
                            i--;
                            break;
                        }
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
                Console.WriteLine("MatchController error: RecreateUser error, error message: " + e.Message);
                queued.Username = "";
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
            if(curUser.Username == candidate.Username) ///if we spam the start and stop searching button
            {
                return false;
            }


            /*MatchUser candidateArch = new MatchUser();
            candidateArch.setArch(candidate);*/

            bool success = (candidate.Age == curUser.Age); ///first step
            success = success && (candidate.Sex == curUser.LookingForSex || curUser.LookingForSex == GENDER.ANY) && DatabaseController.instance().WasntBlockedBy(curUser.Username, candidate.Username);  ///we are cool if the candidate is in the gender we are looking for OR if we dont care about it at all
            success = success && (curUser.Sex == candidate.LookingForSex || candidate.LookingForSex == GENDER.ANY) && DatabaseController.instance().WasntBlockedBy(candidate.Username, curUser.Username); ///and vice versa

            /*if (success == false) ///then add it to the "unmatchable" group.
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
            }*/
            return success;
        }

        private bool removeFromClientList(string username)
        {
            int i = -1;
            lock (llock)
            {                
                MatchUser needle = new MatchUser();
                do
                {
                    i++;
                    foreach (MatchUser user in clients)
                    {
                        if (user.Username == username)
                        {
                            needle = user;
                        }
                    }
                } while (clients.Remove(needle));                              
            }
            return (i > 0);
        }

        private void handleInputCommands()
        {
            while (true)
            {
                //Console.WriteLine("|");
                lock (llock)
                {
                    foreach (MatchUser source in clients)
                    {
                        try
                        {
                            NetworkStream stream = source.Client.GetStream();
                            if (stream.DataAvailable)
                            {

                                KeyValuePair<bool, string> pair = Utility.ReadFromNetworkStream(stream);

                                if (pair.Key == false) { Console.WriteLine("MatchController: invalid syntax on message, discarding request."); continue; }

                                string data = pair.Value;

                                //string data = Utility.ReadFromNetworkStream(stream);

                                if (data[0] == '!')
                                {
                                    handleCommands(data);
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("MatchController: Misc. data from " + source.Username + ", data: " + data);
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("MatchController error: could not reach client, removing from list. Error message: " + e.Message);
                            clients.Remove(source);
                        }
                        
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void handleCommands(string command)
        {
            string[] commandargs = command.Split("|");
            if (commandargs[0] == "!LEAVE" && commandargs.Length == 2)
            {
                if (removeFromClientList(commandargs[1]))
                {
                    Console.WriteLine("MatchController:" + commandargs[1] + " succesfully removed from clientlist!");
                }
                else
                {
                    Console.WriteLine("MatchController:" + commandargs[1] + " couldnt be removed from clientlist!");
                }
            }
            else
            {
                Console.WriteLine("MatchController: unknown command " + command);
            }
        }

        private void ThreadManaging()
        {
            while(true)
            {
                ConcurrentDictionary<PrivateChatController, Thread> AliveThreads = new ConcurrentDictionary<PrivateChatController, Thread>();
                lock(llock)
                {
                    foreach(KeyValuePair<PrivateChatController, Thread> t in privatechatsThreads)
                    {
                        //Console.WriteLine("TEMP: TRY TO REMOVE SHIT");
                        if(t.Key.Ongoing == false)
                        {
                            try
                            {
                                t.Value.Abort();
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine("MatchController: powerkill on PrivateChatThread on port " + t.Key.Portnum);

                            }
                            finally
                            {
                                Thread.Sleep(10);
                                if(t.Key.Type == CHATTPYE.PRIVATE)
                                {
                                    PortManager.instance().ReturnPrivateChatPort(t.Key.Portnum);
                                }
                                else if(t.Key.Type == CHATTPYE.GROUP)
                                {
                                    PortManager.instance().ReturnGroupChatPort(t.Key.Portnum);
                                }
                            }
                        }
                        else
                        {
                            AliveThreads.TryAdd(t.Key, t.Value);
                        }
                    }
                    privatechatsThreads = AliveThreads;
                    
                }

                if(privatechatsThreads.Count == 0)
                {
                    Thread.Sleep(20000);
                }
                else
                {
                    Console.WriteLine("MatchController: alive chats: " + privatechatsThreads.Count);
                    Thread.Sleep(10000);
                }
                
            }
        }

        public TcpListener Server { get { return server; } }
    }
}
