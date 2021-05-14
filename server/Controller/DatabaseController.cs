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
using server.Model;



namespace server.Controller
{
    class DatabaseController : IDatabaseController
    {
        private SqlConnection connection;

        static DatabaseController inst;

        private string userID;
        private string password;
        private string datasource;
        private string initialCatalog;


        private static readonly object registerllock = new object();
        private static readonly object historyllock = new object();
        private static readonly object blockllock = new object();
        private static readonly object friendllock = new object();

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
            //string configpath = @"..\config\database.conf";

            //string src = "Data Source=MSSQLLocalDB";
            //string src = "localhost\\MSSQLLocalDB";
            //string path = "C:\\Users\\Kiss Ádám\\Desktop\\Szakdolgozat\\server\\server\\Database\\KnocKnock.mdf";
            //string path = "/home/adam0801k/server/server/Database/KnocKnock.mdf";

            //string constr = src + ";AttachDbFilename=" + path + ";Integrated Security=True";

            string path = Program.Databaseconfigpath;
            //Console.WriteLine(path);
            userID = "";
            password = "";
            datasource = "";
            initialCatalog = "";


            try
            {
                using (StreamReader sr = new StreamReader(Program.Databaseconfigpath))
                {
                    string line;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        Console.WriteLine("DatabaseController: From config file: " + line);

                        string datamember = line.Split("=")[0];
                        string value = line.Split("=")[1];

                        switch (datamember)
                        {
                            case "UserID":
                                if (userID != "")
                                {
                                    Console.WriteLine("DatabaseController error: UserId already given with value of " + userID + "! Exiting...");
                                    Environment.Exit(0);
                                }
                                else
                                {
                                    userID = value;
                                }
                                break;

                            case "Password":
                                if (password != "")
                                {
                                    Console.WriteLine("DatabaseController error: Password already given with value of " + password + "! Exiting...");
                                    Environment.Exit(0);
                                }
                                else
                                {
                                    password = value;
                                }
                                break;

                            case "DataSource":
                                if (datasource != "")
                                {
                                    Console.WriteLine("DatabaseController error: DataSource already given with value of " + datasource + "! Exiting...");
                                    Environment.Exit(0);
                                }
                                else
                                {
                                    datasource = value;
                                }
                                break;

                            case "InitialCatalog":
                                if (initialCatalog != "")
                                {
                                    Console.WriteLine("DatabaseController error: InitialCatalog already given with value of " + initialCatalog + "! Exiting...");
                                    Environment.Exit(0);
                                }
                                else
                                {
                                    initialCatalog = value;
                                }
                                break;
                        }


                    }

                    if (userID == "")
                    {
                        Console.WriteLine("DatabaseController error: No data provided for UserID. Exiting...");
                        Environment.Exit(0);
                    }
                    if (password == "")
                    {
                        Console.WriteLine("DatabaseController error: No data provided for Password. Exiting...");
                        Environment.Exit(0);
                    }
                    if (datasource == "")
                    {
                        Console.WriteLine("DatabaseController error: No data provided for DataSource. Exiting...");
                        Environment.Exit(0);
                    }
                    if (initialCatalog == "")
                    {
                        Console.WriteLine("DatabaseController error: No data provided for InitialCatalog. Exiting...");
                        Environment.Exit(0);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("DatabaseController error: cannot open config file, error message: " + e.Message + "\nExiting...");
                Environment.Exit(0);
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = datasource;
            builder.UserID = userID;
            builder.Password = password;
            builder.InitialCatalog = initialCatalog;

            try
            {
                Console.WriteLine("DatabaseController: Connecting to SQL Server... ");
                connection = new SqlConnection(builder.ConnectionString);

                connection.Open();
                Console.WriteLine("DatabaseController: Connected to database.");
            }
            catch (Exception e)
            {
                Console.WriteLine("DatabaseController error: cannot connect to database, error message: " + e.Message + "\nExiting... ");
                Environment.Exit(0);
            }


            //string constr = "Data Source=KnocKnock.mdf;AttachDbFilename=/home/adam0801k/server/server/Database/KnocKnock.mdf;Persist Security Info=False";
            //connection = new SqlConnection(constr);
        }
      

        public string GetAgeAndGender(string username)
        {
            lock (registerllock)
            {
                string commandText = "SELECT AGE, GENDER FROM Users WHERE username = @username_param";

                SqlCommand command = new SqlCommand(commandText, connection); ///according to sof, its sanitized

                command.Parameters.AddWithValue("@username_param", username);

                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }
                    /*string rowsAffected = command.ExecuteReader().ToString();
                    Console.WriteLine("RowsAffected: {0}", rowsAffected);*/
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader != null)
                    {
                        reader.Read();
                        string res = String.Format("{0}|{1}", reader[0], reader[1]);
                        reader.Close();


                        return res;
                    }
                    else
                    {
                        Console.WriteLine(username + " was not registered, so couldnt retrive age and gender!");
                        reader.Close();
                        return "";
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return "";
                }
            }

        }

        public bool successfulRegister(string username, string password, int age, int sex)
        {
            lock (registerllock)
            {
                username = username.Replace("'", "\""); ///we wont let them use ' in registration, but better to be safe than sorry
                string commandText = "SELECT COUNT (*) FROM Users WHERE username = @username_param";

                SqlCommand command = new SqlCommand(commandText, connection); ///according to sof, its sanitized

                command.Parameters.AddWithValue("@username_param", username);

                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }
                    /*string rowsAffected = command.ExecuteReader().ToString();
                    Console.WriteLine("RowsAffected: {0}", rowsAffected);*/
                    string res = command.ExecuteScalar().ToString();

                    Console.WriteLine(res);


                    if (res == "0")
                    {
                        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                        int curtime = (int)t.TotalSeconds;
                        string insertText = "INSERT INTO Users Values (@uname, @pwd, NULL, @sex, @age, @curtime)";

                        command = new SqlCommand(insertText, connection);
                        command.Parameters.AddWithValue("@uname", username);
                        command.Parameters.AddWithValue("@pwd", password);
                        command.Parameters.AddWithValue("@age", age);
                        command.Parameters.AddWithValue("@sex", sex);
                        command.Parameters.AddWithValue("@curtime", curtime);
                        command.ExecuteNonQuery();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error in registration. Error message: " + ex.Message);
                    return false;
                }
            }
        }

        public bool successfulLogin(string username, string password)
        {
            lock (registerllock)
            {
                string commandText = "SELECT password FROM Users WHERE username = @username_param";

                SqlCommand command = new SqlCommand(commandText, connection); ///according to sof, its sanitized

                command.Parameters.AddWithValue("@username_param", username);

                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }
                    /*string rowsAffected = command.ExecuteReader().ToString();
                    Console.WriteLine("RowsAffected: {0}", rowsAffected);*/
                    //Console.WriteLine("testdatabase");
                    SqlDataReader reader = command.ExecuteReader();

                    //Console.WriteLine("Reader: " + reader.ToString());
                    if (reader.Read())
                    {
                        string res = String.Format("{0}", reader[0]);
                        string pwd = String.Format("{0}", password);

                        /*Console.WriteLine("DBpassword: " + res + "|");
                        Console.WriteLine("OGpassword: " + password + "|");
                        Console.WriteLine("DB hash: " + res.GetHashCode());
                        Console.WriteLine("OG hash: " + password.GetHashCode());

                        Console.WriteLine("\n" + (pwd.Length == res.Length));
                        for (int i=0; i<pwd.Length; i++)
                        {
                            Console.Write(pwd[i] == res[i]);
                        }*/



                        reader.Close();

                        return (res == password);
                    }
                    else
                    {
                        reader.Close();
                        Console.WriteLine("DatabaseController: " + username + " was not registered, so cannot log in!");
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error: " + ex.Message/* + "\nStactrace: " + ex.StackTrace*/);
                    return false;
                }
            }

        }



        public bool WasntBlockedBy(string blockedby, string blockedCandidate)
        {
            lock (blockllock)
            {
                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }

                    string commandText = "SELECT count(*) FROM BlockList WHERE blocker = @blockedby_param and blocked = @blockedCandidate_param";

                    SqlCommand command = new SqlCommand(commandText, connection); ///according to sof, its sanitized

                    command.Parameters.AddWithValue("@blockedby_param", blockedby);
                    command.Parameters.AddWithValue("@blockedCandidate_param", blockedCandidate);

                    bool wasntblocked = false;
                    try
                    {
                        if (!(connection.State == ConnectionState.Open))
                        {
                            connection.Open();
                        }
                        /*string rowsAffected = command.ExecuteReader().ToString();
                        Console.WriteLine("RowsAffected: {0}", rowsAffected);*/
                        //Console.WriteLine("testdatabase");
                        SqlDataReader reader = command.ExecuteReader();

                        try
                        {
                            Console.WriteLine("DatabaseController: checking if " + blockedCandidate + " was blocked by " + blockedby);
                            reader.Read();
                            string res = String.Format("{0}", reader[0]);
                            wasntblocked = (res == "0");

                            //Console.WriteLine("Blocked: " + res);
                        }
                        catch (Exception f)
                        {
                            Console.WriteLine("DatabaseController notice: error during reading, probably reading is not finished. Closing reader and returning result... Error message: " + f);
                        }


                        reader.Close();



                        return wasntblocked;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("DatabaseController error in blocklist checking. Error message: " + ex.Message);
                        return false; ///if there's an exception, just dont match them.
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error in blocklist checking. Error message: " + ex.Message);
                    return false; ///if there's an exception, just dont match them.
                }
            }
        }

        public bool BlockUser(string blocker, string blocked)
        {
            lock(blockllock)
            {
                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }
                    string insertText = "INSERT INTO Blocklist Values (@blockername, @blockedname)";

                    SqlCommand command = new SqlCommand(insertText, connection);
                    command.Parameters.AddWithValue("@blockername", blocker);
                    command.Parameters.AddWithValue("@blockedname", blocked);


                    command.ExecuteNonQuery();


                    string deleteText = "DELETE FROM Friendlist where sender = @usernameparam";
                    command = new SqlCommand(deleteText, connection);
                    command.Parameters.AddWithValue("@usernameparam", blocker);
                    command.ExecuteNonQuery();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error in blocklist insertion. Error message: " + ex.Message);
                    return false;
                }
            }
        }

        public bool FriendUser(string friender, string friended)
        {
            lock (friendllock)
            {
                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }

                    if(WasntAlreadyFriended(friender,friended))
                    {
                        string insertText = "INSERT INTO FriendList Values (@friender_name, @friended_name)";

                        SqlCommand command = new SqlCommand(insertText, connection);
                        command.Parameters.AddWithValue("@friender_name", friender);
                        command.Parameters.AddWithValue("@friended_name", friended);


                        command.ExecuteNonQuery();                        
                    }

                    return true;

                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error in friendlist insertion. Error message: " + ex.Message);
                    return false;
                }
            }
        }

        private bool WasntAlreadyFriended(string friender, string friended)
        {
            lock(friendllock)
            {
                bool wasntAlreadyFriended = true;

                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }
                    string commandText = "SELECT COUNT(*) FROM friendlist where sender=@friender_name and befriended=@friended_name";

                    SqlCommand command = new SqlCommand(commandText, connection);
                    command.Parameters.AddWithValue("@friender_name", friender);
                    command.Parameters.AddWithValue("@friended_name", friended);


                    SqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        Console.WriteLine("DatabaseController: checking if " + friended + " was friended by " + friender);
                        reader.Read();
                        string res = String.Format("{0}", reader[0]);
                        wasntAlreadyFriended = (res == "0");

                        Console.WriteLine("TEMP: Friended: " + res);
                    }
                    catch (Exception f)
                    {
                        Console.WriteLine("DatabaseController notice: error during reading, probably reading is not finished. Closing reader and returning result... Error message: " + f);
                    }


                    reader.Close();

                    return wasntAlreadyFriended;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error in friendlist insertion. Error message: " + ex.Message);
                    return false;
                }
            }           
        }


        public bool InsertMessageHistoryConnection(string messagehistoryname, string inserter)
        {
            lock(historyllock)
            {
                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }
                    TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                    int curtime = (int)t.TotalSeconds;
                    string insertText = "INSERT INTO HistoryConnector Values (@hname, @uname)";
                    Console.WriteLine("DatabaseController: inserting connection to messagehistory entry " + messagehistoryname + "for user " + inserter);

                    SqlCommand command = new SqlCommand(insertText, connection);
                    command.Parameters.AddWithValue("@hname", messagehistoryname);
                    command.Parameters.AddWithValue("@uname", inserter);
                    

                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error in messagehistory connection insertion. Error message: " + ex.Message);
                    return false;
                }
            }
        }

        public bool InsertMessageHistoryText(string messagehistoryname, string text)
        {
            lock(historyllock)
            {
                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }
                    string insertText = "INSERT INTO MessageHistory Values (@hname, @text)";
                    Console.WriteLine("DatabaseController: inserting text for messagehistory entry " + messagehistoryname);

                    SqlCommand command = new SqlCommand(insertText, connection);
                    command.Parameters.AddWithValue("@hname", messagehistoryname);
                    command.Parameters.AddWithValue("@text", text);

                    int res = command.ExecuteNonQuery();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error in messagehistory connection insertion. Error message: " + ex.Message);
                    return false;
                }
            }
        }

        public bool AlreadySavedChatHistory(string historyname)
        {
            lock (historyllock)
            {
                string commandText = "SELECT count(*) FROM MessageHistory WHERE MessageHistoryname = @historyname_param";
                SqlCommand command = new SqlCommand(commandText, connection); ///according to sof, its sanitized

                command.Parameters.AddWithValue("@historyname_param", historyname);

                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }
                    /*string rowsAffected = command.ExecuteReader().ToString();
                    Console.WriteLine("RowsAffected: {0}", rowsAffected);*/
                    //Console.WriteLine("testdatabase");
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Read();

                    string res = String.Format("{0}", reader[0]);
                    Console.WriteLine("DatabaseController: " + historyname + " was inserted " + res + "times!");

                    reader.Close();

                    int cnt = int.Parse(res);

                    return cnt > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error: cannot retrieve wether if" + historyname + "was saved, error message: " + ex.Message/* + "\nStactrace: " + ex.StackTrace*/);
                    return false;
                }
            }            
        }

        public List<string> GetChatHistoryIDs(string username)
        {
            lock (historyllock)
            {
                string commandText = "SELECT chatname FROM HistoryConnector WHERE saver = @username_param ORDER BY chatname";
                SqlCommand command = new SqlCommand(commandText, connection); ///according to sof, its sanitized

                command.Parameters.AddWithValue("@username_param", username);

                List<string> reslist = new List<string>();

                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }
                    /*string rowsAffected = command.ExecuteReader().ToString();
                    Console.WriteLine("RowsAffected: {0}", rowsAffected);*/
                    //Console.WriteLine("testdatabase");
                    SqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        Console.WriteLine("DatabaseController: retireving things from HistoryConnector where saver = " + username);
                        while (reader.Read())
                        {
                            string res = String.Format("{0}", reader[0]);
                            Console.WriteLine("DatabaseController: " + res + " was saved by " + username + "!");
                            reslist.Add(res);

                        }

                        //Console.Write("THE LIST: " + reslist + "\n");
                    }
                    catch(Exception f)
                    {
                        Console.WriteLine("DatabaseController notice: error during reading, probably reading is not finished. Closing reader and returning result... Error message: " + f.Message);
                    }
                    

                    reader.Close();

                    return reslist;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error: cannot retrieve the saveIDs of" + username + ", error message: " + ex.Message/* + "\nStactrace: " + ex.StackTrace*/);
                    return reslist;
                }
            }
        }

        public List<string> GetMutualFriending(string username)
        {
            List<string> reslist = new List<string>();


            lock (friendllock)
            {
                string commandText = "select distinct befriended from friendlist  where sender = @usernameparam   intersect select sender from friendlist where befriended = @usernameparam";

                SqlCommand command = new SqlCommand(commandText, connection); ///according to sof, its sanitized

                command.Parameters.AddWithValue("@usernameparam", username);


                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }
                    /*string rowsAffected = command.ExecuteReader().ToString();
                    Console.WriteLine("RowsAffected: {0}", rowsAffected);*/
                    //Console.WriteLine("testdatabase");
                    SqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        while (reader.Read())
                        {
                            string res = String.Format("{0}", reader[0]);
                            Console.WriteLine("DatabaseController: " + res + " and " + username + " are mutual friends!");
                            reslist.Add(res);

                        }

                        //Console.Write("THE LIST: " + reslist + "\n");
                    }
                    catch (Exception f)
                    {
                        Console.WriteLine("DatabaseController warning: error during reading, probably reading is not finished. Closing reader and returning result... Error message: " + f.Message);
                    }


                    reader.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error: cannot retrieve the saveIDs of" + username + ", error message: " + ex.Message/* + "\nStactrace: " + ex.StackTrace*/);
                }
            }


            return reslist;
        }
        
        public List<string> GetOnlySenderLovedBy(string username)
        {
            List<string> reslist = new List<string>();


            lock (friendllock)
            {
                string commandText = "select distinct sender from friendlist where befriended = @usernameparam except ( select befriended from friendlist where sender = @usernameparam intersect select sender from friendlist where befriended = @usernameparam )";

                SqlCommand command = new SqlCommand(commandText, connection); ///according to sof, its sanitized

                command.Parameters.AddWithValue("@usernameparam", username);


                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }
                    /*string rowsAffected = command.ExecuteReader().ToString();
                    Console.WriteLine("RowsAffected: {0}", rowsAffected);*/
                    //Console.WriteLine("testdatabase");
                    SqlDataReader reader = command.ExecuteReader();

                    try
                    {
                        while (reader.Read())
                        {
                            string res = String.Format("{0}", reader[0]);
                            Console.WriteLine("DatabaseController: " + res + " left-friend " + username + "!");
                            reslist.Add(res);

                        }

                        //Console.Write("THE LIST: " + reslist + "\n");
                    }
                    catch (Exception f)
                    {
                        Console.WriteLine("DatabaseController warning: error during reading, probably reading is not finished. Closing reader and returning result... Error message: " + f.Message);
                    }


                    reader.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error: cannot retrieve the saveIDs of" + username + ", error message: " + ex.Message/* + "\nStactrace: " + ex.StackTrace*/);
                }
            }
            return reslist;
        }

        public List<string> GetOnlyLovedBySender(string username)
        {
            List<string> reslist = new List<string>();


            lock (friendllock)
            {
                string commandText = "select distinct befriended from friendlist where sender = @usernameparam except ( select befriended from friendlist where sender = @usernameparam intersect select sender from friendlist where befriended = @usernameparam )";

                SqlCommand command = new SqlCommand(commandText, connection); ///according to sof, its sanitized

                command.Parameters.AddWithValue("@usernameparam", username);


                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }
                    /*string rowsAffected = command.ExecuteReader().ToString();
                    Console.WriteLine("RowsAffected: {0}", rowsAffected);*/
                    //Console.WriteLine("testdatabase");
                    SqlDataReader reader = command.ExecuteReader();

                    try
                    {                        
                        while (reader.Read())
                        {
                            string res = String.Format("{0}", reader[0]);
                            Console.WriteLine("DatabaseController: " + res + " right-friend " + username + "!");
                            reslist.Add(res);

                        }

                        //Console.Write("THE LIST: " + reslist + "\n");
                    }
                    catch (Exception f)
                    {
                        Console.WriteLine("DatabaseController warning: error during reading, probably reading is not finished. Closing reader and returning result... Error message: " + f.Message);
                    }


                    reader.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error: cannot retrieve the saveIDs of" + username + ", error message: " + ex.Message/* + "\nStactrace: " + ex.StackTrace*/);
                }
            }
            return reslist;
        }

        public string GetChatHistoryText(string historyID)
        {
            lock (historyllock)
            {
                string commandText = "SELECT text FROM messagehistory WHERE messagehistoryname = @historyname_param";
                SqlCommand command = new SqlCommand(commandText, connection); ///according to sof, its sanitized

                command.Parameters.AddWithValue("@historyname_param", historyID);
                //string[] reslist = new string[0];

                try
                {
                    if (!(connection.State == ConnectionState.Open))
                    {
                        connection.Open();
                    }
                    /*string rowsAffected = command.ExecuteReader().ToString();
                    Console.WriteLine("RowsAffected: {0}", rowsAffected);*/
                    //Console.WriteLine("testdatabase");
                    SqlDataReader reader = command.ExecuteReader();
                    string res = "ER";

                    try
                    {                       
                        if (reader.Read())
                        {
                            res = String.Format("{0}", reader[0]);
                            //Console.WriteLine("DatabaseController: " + res + " was the text of  " + historyID + "!");
                            Console.WriteLine("DatabaseController: getting the text of entry " + historyID);
                        }
                        else
                        {
                            Console.WriteLine("DatabaseController notice: there is no text for" + historyID + ", sending ER!");
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("DatabaseController error: error during reading chathistory text. Error message: " + e.Message);
                    }
                    


                    reader.Close();
                    return res;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DatabaseController error: cannot retrieve the text of" + historyID + ", error message: " + ex.Message/* + "\nStactrace: " + ex.StackTrace*/);
                    return "";
                }
            }
        }

        
    
    }
    
}
