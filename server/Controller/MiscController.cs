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
using server.Controller;

namespace server.Controller
{
    class MiscController
    {
        private static readonly object llock = new object();
        TcpListener server;

        private static MiscController inst;

        private MiscController() { }

        public static MiscController instance()
        {
            if(inst == null)
            {
                inst = new MiscController();
            }
            return inst;
        }

        public void handleRequests()
        {
            server = new TcpListener(IPAddress.Any, PortManager.instance().Miscport);
            server.Start();

            while(true)
            {
                TcpClient client = server.AcceptTcpClient();

                try
                {
                    NetworkStream stream = client.GetStream();

                    KeyValuePair<bool, string> pair = Utility.ReadFromNetworkStream(stream);

                    if (pair.Key == false) { Console.WriteLine("MiscController: invalid syntax on message, discarding request."); continue; }

                    string command = pair.Value;

                    //string command = Utility.ReadFromNetworkStream(stream);
                    handleCommands(command, stream);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Error in MiscManager, error message: " + e.Message);
                }
            }
        }

        private void handleCommands(string command, NetworkStream stream)
        {
            string[] commandargs = command.Split("|");

            if(commandargs[0] == "CONVSAVE")
            {
                ConvSave(commandargs, stream);
            }
            else if(commandargs[0] == "CONVLOAD")
            {
                ConvLoad(commandargs, stream);
            }
            else if (commandargs[0] == "LISTLOAD")
            {
                ListLoad(commandargs, stream);                
            }
            else if(commandargs[0] == "BLOCK")
            {
                Block(commandargs, stream);
            }
            else if(commandargs[0] == "FRIEND")
            {
                Friend(commandargs, stream);
            }
            else if (commandargs[0] == "FRIENDLOAD")
            {
                FriendLoad(commandargs, stream);
            }
        }


        private void ConvSave(string[] commandargs, NetworkStream stream)
        {
            string savename = commandargs[1] + "|" + commandargs[2] + "|" + commandargs[3]; ;
            string inserter = commandargs[4];
            bool wasSaved = DatabaseController.instance().AlreadySavedChatHistory(savename);

            if (wasSaved)
            {
                try
                {
                    bool success = DatabaseController.instance().InsertMessageHistoryConnection(savename, inserter);
                    Console.WriteLine("MiscController: already inserted message history for " + inserter + " as " + savename);

                    if (success)
                    {
                        byte[] okmsg = Encoding.Unicode.GetBytes("OK");
                        stream.Write(okmsg);
                    }
                    else
                    {
                        byte[] okmsg = Encoding.Unicode.GetBytes("ER");
                        stream.Write(okmsg);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("MiscController error: could not reach client, error message: " + e.Message);
                    return;
                }

            }
            else
            {
                try
                {
                    byte[] okmsg = Encoding.Unicode.GetBytes("INSERT");
                    stream.Write(okmsg);


                    Thread.Sleep(100);
                    //Console.WriteLine("reading history...");
                    KeyValuePair<bool, string> pair = Utility.ReadFromNetworkStream(stream);

                    if (pair.Key == false) { Console.WriteLine("MiscController: invalid syntax on message, discarding request."); return; }

                    string history = pair.Value;


                    //string history = Utility.ReadFromNetworkStream(stream);
                    if (DatabaseController.instance().InsertMessageHistoryConnection(savename, inserter))
                    {
                        if (DatabaseController.instance().InsertMessageHistoryText(savename, history))
                        {
                            okmsg = Encoding.Unicode.GetBytes("OK");
                            stream.Write(okmsg);
                            Console.WriteLine("MiscController: inserted message history for " + inserter + " as " + savename + " with text\n" + history);
                        }
                        else
                        {
                            okmsg = Encoding.Unicode.GetBytes("ER");
                            Console.WriteLine("MiscController notice: could not insert history text! Consider deleting manually the connections!");
                            stream.Write(okmsg);
                        }
                    }
                    else
                    {
                        Console.WriteLine("MiscController notice: could not insert history connection!");
                        okmsg = Encoding.Unicode.GetBytes("ER");
                        stream.Write(okmsg);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("MiscController notice: could not reach client after inserting history text, exception message: " + e.Message);
                    return;
                }
            }
        }
        private void ConvLoad(string[] commandargs, NetworkStream stream)
        {
            try
            {
                string convid = commandargs[1] + "|" + commandargs[2] + "|" + commandargs[3];
                string res = DatabaseController.instance().GetChatHistoryText(convid);

                byte[] reply = Encoding.Unicode.GetBytes(res);

                stream.Write(reply);
            }
            catch (Exception e)
            {
                Console.WriteLine("MiscController error: could not send convtext message to client! Error message: " + e.Message);
            }
        }
        private void ListLoad(string[] commandargs, NetworkStream stream)
        {
            try
            {
                string username = commandargs[1];
                Console.WriteLine("MiscController: trying to get the list of " + username);

                List<string> reslist = DatabaseController.instance().GetChatHistoryIDs(username);
                string res = String.Join("!", reslist);

                /*foreach(string thingy in res)
                {
                    Console.Write(thingy + "   ");
                }

                string answer = String.Join("!", res);
                answer = answer + "!";*/

                byte[] reply = Encoding.Unicode.GetBytes(res);

                stream.Write(reply);
            }
            catch (Exception e)
            {
                Console.WriteLine("MiscController error: could not send back convlist message to client! Error message: " + e.Message);
            }
        }
        private void Block(string[] commandargs, NetworkStream stream)
        {
            string blocker = commandargs[1];
            string blocked = commandargs[2];
            try
            {
                bool success = DatabaseController.instance().BlockUser(blocker, blocked);
                Console.WriteLine("MiscController: blocking " + blocked + " by " + blocker);

                if (success)
                {
                    byte[] okmsg = Encoding.Unicode.GetBytes("OK");
                    stream.Write(okmsg);
                }
                else
                {
                    byte[] okmsg = Encoding.Unicode.GetBytes("ER");
                    stream.Write(okmsg);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("MiscController error: could not reach client, error message: " + e.Message);
                return;
            }
        }
        private void Friend(string[] commandargs, NetworkStream stream)
        {
            string friender = commandargs[1];
            string friended = commandargs[2];
            try
            {
                bool success = DatabaseController.instance().FriendUser(friender, friended);
                Console.WriteLine("MiscController: adding " + friended + " as friend for " + friender);

                if (success)
                {
                    byte[] okmsg = Encoding.Unicode.GetBytes("OK");
                    stream.Write(okmsg);
                }
                else
                {
                    byte[] okmsg = Encoding.Unicode.GetBytes("ER");
                    stream.Write(okmsg);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("MiscController error: could not reach client, error message: " + e.Message);
                return;
            }
        }

        private void FriendLoad(string[] commandargs, NetworkStream stream)
        {
            string username = commandargs[1];

            try
            {
                Console.WriteLine("MiscController: loading the firendlist of " + username);
                List<string> mutualLikeList = DatabaseController.instance().GetMutualFriending(username);
                List<string> onlyLovedBySenderList = DatabaseController.instance().GetOnlyLovedBySender(username);
                List<string> onlySenderLovedByList = DatabaseController.instance().GetOnlySenderLovedBy(username);


                string replypart1 = String.Join("|", mutualLikeList);
                string replypart2 = String.Join("|", onlyLovedBySenderList);
                string replypart3 = String.Join("|", onlySenderLovedByList);

                string reply = replypart1 + "!" + replypart2 + "!" + replypart3;

                //Console.WriteLine("MiscController TEMP: " + reply);

                try
                {                    

                    byte[] msg = Encoding.Unicode.GetBytes(reply);
                    stream.Write(msg);

                }
                catch (Exception e)
                {
                    Console.WriteLine("MiscController error: could not reach client, error message: " + e.Message);
                    return;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("MiscController error: could not reach client, error message: " + e.Message);
                return;
            }
        }
    }
}
