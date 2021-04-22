using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using server.Model;


namespace server.Controller
{
    class DatabaseController
    {
        private SqlConnection connection;

        static DatabaseController inst;

        private static readonly object llock = new object();

        public static DatabaseController instance()
        {
            if(inst == null)
            {
                inst = new DatabaseController();
            }

            return inst;
        }


        private DatabaseController()
        {
            string src = "Data Source=(LocalDB)\\MSSQLLocalDB"; 
            string path = "C:\\Users\\Kiss Ádám\\Desktop\\Szakdolgozat\\server\\server\\Database\\KnocKnock.mdf";

            string constr = src + ";AttachDbFilename=" + path + ";Integrated Security=True";


            connection = new SqlConnection(constr);
        }

        /*public MatchUser loadSUser(string username, string password)
        {
            MatchUser dsUser = new MatchUser();


            return dsUser;
        }*/


        public bool successfulLogin(string username, string password)
        {
            lock(llock)
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
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Read();
                    if (reader != null)
                    {
                        string res = String.Format("{0}", reader[0]);

                        Console.WriteLine("DBpassword: " + res + "|");
                        Console.WriteLine("OGpassword: " + password + "|");
                        reader.Close();

                        return (res == password);
                    }
                    else
                    {
                        Console.WriteLine("not registered!");
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Database error: " + ex.Message);
                    return false;
                }
            }
            
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
                        string res = String.Format("{0}|{1}", reader[0],reader[1]);

                        reader.Close();

                        return res;
                    }
                    else
                    {
                        Console.WriteLine("not registered, so couldnt retrive age and gender!");
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
            lock(llock)
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
