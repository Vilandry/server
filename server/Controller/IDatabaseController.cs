using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.Controller
{
    public interface IDatabaseController
    {
        public string GetAgeAndGender(string username);

        public bool successfulRegister(string username, string password, int age, int sex);

        public bool successfulLogin(string username, string password);


        public bool WasntBlockedBy(string blockedby, string blockedCandidate);

        public bool BlockUser(string blocker, string blocked);

        public bool FriendUser(string friender, string friended);


        public bool InsertMessageHistoryConnection(string messagehistoryname, string inserter);

        public bool InsertMessageHistoryText(string messagehistoryname, string text);

        public bool AlreadySavedChatHistory(string historyname);

        public List<string> GetChatHistoryIDs(string username);

        public List<string> GetMutualFriending(string username);

        public List<string> GetOnlySenderLovedBy(string username);

        public List<string> GetOnlyLovedBySender(string username);

        public string GetChatHistoryText(string historyID);
    }
}
