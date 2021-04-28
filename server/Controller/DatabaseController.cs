﻿using System;
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
    class DatabaseController
    {
        private SqlConnection connection;

        static DatabaseController inst;

        private string userID;
        private string password;
        private string datasource;
        private string initialCatalog;


        private static readonly object llock = new object();

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

        /*public MatchUser loadSUser(string username, string password)
        {
            MatchUser dsUser = new MatchUser();


            return dsUser;
        }*/

        public bool alreadySavedChatHistory(string historyname)
        {
            bool alreadySaved = true;

            string commandText = "SELECT COUNT(*) FROM HistoryConnector WHERE ChatName = @historyname_param";
            SqlCommand command = new SqlCommand(commandText, connection);
            command.Parameters.AddWithValue("@historyname_param", historyname);
            try
            {
                if (!(connection.State == ConnectionState.Open))
                {
                    connection.Open();
                }

                SqlDataReader reader = command.ExecuteReader();
                reader.Read();
                if (reader != null)
                {
                    string res = String.Format("{0}", reader[0]);
                    int numberOfInserted = int.Parse(res);
                    Console.WriteLine("History was inserted " + numberOfInserted + " times. (res: " + res + ")");

                    /*Console.WriteLine("DBpassword: " + res + "|");
                    Console.WriteLine("OGpassword: " + password + "|");*/
                    reader.Close();

                    return (numberOfInserted > 0);
                }
                else
                {
                    reader.Close();
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Database error: " + ex.Message);
                return false;
            }

            return alreadySaved;
        }

        public bool successfulLogin(string username, string password)
        {
            lock (llock)
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
                        Console.WriteLine("DB tostring hash: " + res.ToString().GetHashCode());
                        Console.WriteLine("OG tostring hash: " + password.ToString().GetHashCode());
                        Console.WriteLine("OG formatted hash: " + pwd.GetHashCode());

                        Console.WriteLine("\n" + (pwd.Length == res.Length));
                        for (int i=0; i<pwd.Length; i++)
                        {
                            Console.Write(pwd[i] == res[i]);
                        }*/



                        reader.Close();

                        return (Utility.passwordEquals(res, password));
                    }
                    else
                    {
                        reader.Close();
                        Console.WriteLine(username + " was not registered!");
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Database error: " + ex.Message + "\nStactrace: " + ex.StackTrace);
                    return false;
                }
            }

        }

        public bool WasntBlockedBy(string blockedby, string blockedCandidate)
        {
            return true;
        }

        public string GetAgeAndGender(string username)
        {
            lock (llock)
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
            lock (llock)
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
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
    }
}
