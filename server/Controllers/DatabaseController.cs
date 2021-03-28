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


namespace server.Controllers
{
    class DatabaseController
    {
        private SqlConnection connection;

        static DatabaseController inst;

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

        public SUser loadSUser(string username, string password)
        {
            SUser dsUser = new SUser();


            return dsUser;
        }


        public bool successfulLogin(string username, string password)
        {
            bool correctmatch = false;
            string commandText = "SELECT password FROM Users WHERE username = @username_param";

            SqlCommand command = new SqlCommand(commandText, connection); ///according to sof, its sanitized

            command.Parameters.AddWithValue("@username_param", username);

            try
            {
                connection.Open();
                /*string rowsAffected = command.ExecuteReader().ToString();
                Console.WriteLine("RowsAffected: {0}", rowsAffected);*/
                object o = command.ExecuteScalar();

                if(o != null)
                {
                    string res = o.ToString();

                    Console.WriteLine(res);


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
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool successfulRegister(string username, string password, int age, int sex)
        {
            username = username.Replace("'", "\""); ///we wont let them use ' in registration, but better to be safe than sorry
            string commandText = "SELECT COUNT (*) FROM Users WHERE username = @username_param";

            SqlCommand command = new SqlCommand(commandText, connection); ///according to sof, its sanitized

            command.Parameters.AddWithValue("@username_param", username); 

            try
            {
                connection.Open();
                /*string rowsAffected = command.ExecuteReader().ToString();
                Console.WriteLine("RowsAffected: {0}", rowsAffected);*/
                string res = command.ExecuteScalar().ToString();
    
                Console.WriteLine(res);


                if(res == "0")
                {
                    string insertText = "INSERT INTO Users Values (@uname, @pwd, NULL, @age, @sex)";

                    command = new SqlCommand(insertText, connection);
                    command.Parameters.AddWithValue("@uname", username);
                    command.Parameters.AddWithValue("@pwd", password);
                    command.Parameters.AddWithValue("@age", age);
                    command.Parameters.AddWithValue("@sex", sex);
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
