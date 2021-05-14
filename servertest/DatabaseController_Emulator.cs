using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using servertest.Model;
using servertest.Controller;



namespace servertest
{
    public class DatabaseController : server.Controller.IDatabaseController
    {
        static DatabaseController inst;

        public static DatabaseController instance()
        {
            if (inst == null)
            {
                inst = new DatabaseController();
            }

            return inst;
        }


        private DatabaseController()
        {

        }


        public string GetAgeAndGender(string username)
        {
            if(username == "NotTaken") { return "0|1"; }
            return "";
        }

        public bool successfulRegister(string username, string password, int age, int sex)
        {
            ///Console.WriteLine((username == "NotTaken") + " was the res");
            return (username == "NotTaken");
        }

        public bool successfulLogin(string username, string password)
        {
            if(username == "NotTaken" && password == "nottaken") { return true; }

            return false;

        }



        public bool WasntBlockedBy(string blockedby, string blockedCandidate)
        {
            return false;
        }

        public bool BlockUser(string blocker, string blocked)
        {
            return false;
        }

        public bool FriendUser(string friender, string friended)
        {
            return false;
        }

        private bool WasntAlreadyFriended(string friender, string friended)
        {
            return false;
        }


        public bool InsertMessageHistoryConnection(string messagehistoryname, string inserter)
        {
            Console.WriteLine(messagehistoryname + "\n" + inserter);
            if (messagehistoryname == "0|friend|NotTaken" && inserter == "friend") { return true; }
            if (messagehistoryname == "1|friend|NotTaken" && inserter == "NotTaken") { return true; }
            
            return false;
        }

        public bool InsertMessageHistoryText(string messagehistoryname, string text)
        {
            if(messagehistoryname == "0|friend|NotTaken|") { return true; }
            return true;
        }

        public bool AlreadySavedChatHistory(string historyname)
        {
            if (historyname == "0|friend|NotTaken|") { return false; }
            if (historyname == "1|friend|NotTaken") { return true; }
            

            return false;
        }

        public List<string> GetChatHistoryIDs(string username)
        {
            return null;
        }

        public List<string> GetMutualFriending(string username)
        {
            if(username == "friend")
            {
                List<string> list = new List<string>();
                list.Add("mutual");
                return list;
            }
            
            return null;
        }

        public List<string> GetOnlySenderLovedBy(string username)
        {
            if (username == "friend")
            {
                List<string> list = new List<string>();
                list.Add("OnlyTarget");
                return list;
            }
            return null;
        }

        public List<string> GetOnlyLovedBySender(string username)
        {
            if (username == "friend")
            {
                List<string> list = new List<string>();
                list.Add("OnlySender");
                return list;
            }
            return null;
        }

        public string GetChatHistoryText(string historyID)
        {
            Console.WriteLine("dbcont: " + historyID);
            if(historyID == "0|friend|NotTaken") { return "ok"; }
            return "";
        }



    }

}
