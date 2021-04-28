using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;

namespace server.Model
{
    public class PortManager
    {
        private static PortManager manager;
        private static readonly object llock = new object();
        ConcurrentQueue<int> privateChatPorts;
        ConcurrentQueue<int> groupChatPorts;
        HashSet<int> takenPorts;
        private int matchport;
        private int loginport;
        private const int infoport = 9000;
        private int miscport;


        public static PortManager instance()
        {
            if(manager==null)
            {
                manager = new PortManager();
                
            }

            return manager;
        }

        private PortManager()
        {
            privateChatPorts = new ConcurrentQueue<int>();
            groupChatPorts = new ConcurrentQueue<int>();

            try
            {

                // Open the text file using a stream reader.
                if(Program.Portconfigpath != "")
                {
                    Console.WriteLine("Portmanager: Given config file: " + Program.Portconfigpath);
                    using (StreamReader sr = new StreamReader(Program.Portconfigpath))
                    {
                        matchport = -1;
                        loginport = -1;
                        miscport = -1;

                        takenPorts = new HashSet<int>();
                        string line;
                        // Read and display lines from the file until the end of
                        // the file is reached.
                        while ((line = sr.ReadLine()) != null)
                        {
                            Console.WriteLine("Portmanager: From config file: " + line);

                            string arg = line.Split("=")[0];
                            string[] portrange = line.Split("=")[1].Split("-");

                            try
                            {
                                switch (arg)
                                {
                                    case "matchport":
                                        if (portrange.Length > 1)
                                        {
                                            Console.WriteLine("Portmanager error: Cannot use range for matchport! Exiting program...");
                                            Environment.Exit(0);
                                            Console.ReadKey();
                                        }
                                        else
                                        {
                                            if (!InsertSinglePort(portrange[0], ref matchport))
                                            {
                                                Environment.Exit(0);
                                                Console.ReadKey();
                                            }
                                        }
                                        break;

                                    case "loginport":
                                        if (portrange.Length > 1)
                                        {
                                            Console.WriteLine("Portmanager error: Cannot use range for matchport! Exiting program...");
                                            Environment.Exit(0);
                                            Console.ReadKey();
                                        }
                                        else
                                        {
                                            if (!InsertSinglePort(portrange[0], ref loginport))
                                            {
                                                Environment.Exit(0);
                                                Console.ReadKey();
                                            }
                                        }
                                        break;

                                    case "miscport":
                                        if (portrange.Length > 1)
                                        {
                                            Console.WriteLine("Portmanager error: Cannot use range for matchport! Exiting program...");
                                            Environment.Exit(0);
                                            Console.ReadKey();
                                        }
                                        else
                                        {
                                            if (!InsertSinglePort(portrange[0], ref miscport))
                                            {
                                                Environment.Exit(0);
                                                Console.ReadKey();
                                            }
                                        }
                                        break;

                                    case "infoport":
                                        Console.WriteLine("Portmanager error: Infoport is reserved, and cannot be set! Exiting...");
                                        Environment.Exit(0);
                                        Console.ReadKey();
                                        break;


                                    case "privateChatPorts":
                                        if (portrange.Length != 2)
                                        {
                                            Console.WriteLine("Portmanager error: privateChatPorts must be a range of ports! Exiting...");
                                            Environment.Exit(0);
                                            Console.ReadKey();
                                        }
                                        else
                                        {
                                            string lower = portrange[0];
                                            string upper = portrange[1];
                                            if (!InsertPortRange(lower, upper, privateChatPorts))
                                            {
                                                Environment.Exit(0);
                                                Console.ReadKey();
                                            }
                                        }
                                        break;

                                    case "groupChatPorts":
                                        if (portrange.Length != 2)
                                        {
                                            Console.WriteLine("Portmanager error: groupChatPorts must be a range of ports! Exiting...");
                                            Environment.Exit(0);
                                            Console.ReadKey();
                                        }
                                        else
                                        {
                                            string lower = portrange[0];
                                            string upper = portrange[1];
                                            if (!InsertPortRange(lower, upper, groupChatPorts))
                                            {
                                                Environment.Exit(0);
                                                Console.ReadKey();
                                            }
                                        }
                                        break;

                                    default:
                                        Console.WriteLine("Portmanager warning: Unknown porttype " + arg + "! Value is discarded!");
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Portmanager exception: Exception during processing " + arg + ". Could not retrieve data from configfile, using default value on that field. Exception message: " + e.Message);
                            }
                        }
                    }

                    if(matchport == -1 && takenPorts.Contains(9900))
                    {
                        Console.WriteLine("Portmanager error! No matchport given and its default value is assigned! Exiting...");
                        Environment.Exit(0);
                        Console.ReadKey();
                    }
                    else
                    {
                        matchport = 9900;
                        Console.WriteLine("Portmanager notice: No matchport given, using its default value of 9900.");
                    }

                    if (loginport == -1 && takenPorts.Contains(11000))
                    {
                        Console.WriteLine("Portmanager error! No loginport given and its default value is assigned! Exiting...");
                        Environment.Exit(0);
                        Console.ReadKey();
                    }
                    else
                    {
                        loginport = 11000;
                        Console.WriteLine("Portmanager notice: No loginport given, using its default value of 11000.");
                    }

                    if (miscport == -1 && takenPorts.Contains(9899))
                    {
                        Console.WriteLine("Portmanager error! No loginport given and its default value is assigned! Exiting...");
                        Environment.Exit(0);
                        Console.ReadKey();
                    }
                    else
                    {
                        miscport = 9899;
                        Console.WriteLine("Portmanager notice: No miscport given, using its default value of 9899.");
                    }
                    
                    if(privateChatPorts.Count == 0)
                    {
                        Console.WriteLine("Portmanager notice: No privateChatPorts given, using its default value of range 11001-19999.");
                    }
                }
                else
                {
                    Console.WriteLine("Portmanager notice: No config file, using default values");
                    ///fill the privateChatPorts. Should get those numbers from a config file
                    for (int i = 11001; i < 19999; i++)
                    {
                        privateChatPorts.Enqueue(i);
                    }

                    for (int i = 21001; i < 29999; i++)
                    {
                        groupChatPorts.Enqueue(i);
                    }

                    matchport = 9900;
                    loginport = 11000;
                    miscport = 9899;

                    Console.WriteLine("PortManager: default ports, as:\n\tmatchport: 9900\n\tloginport: 11000\n\tmiscport: 9899\n\tinfoport: 9000\n\tprivateChatPorts: 11001-19999\n\tgroupChatPorts: 21001-29999");
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Portmanager exception: The config file could not be read, error message: " + e.Message + "Using default ports!");
                ///fill the privateChatPorts. Should get those numbers from a config file
                for (int i = 11001; i < 19999; i++)
                {
                    privateChatPorts.Enqueue(i);
                }

                for (int i = 21001; i < 29999; i++)
                {
                    groupChatPorts.Enqueue(i);
                }

                matchport = 9900;
                loginport = 11000;
                miscport = 9899;

                Console.WriteLine("PortManager: using default ports, as:\n\tmatchport: 9900\n\tloginport: 11000\n\tmiscport: 9899\n\tinfoport: 9000\n\tprivateChatPorts: 11001-19999\n\tgroupChatPorts: 21001-29999");
            }
        }

        public int GetPrivateChatPort()
        {
            int port;
            if (privateChatPorts.TryDequeue(out port))
            {
                return port;                
            }
            else
            {
                return -1; 
            }
        }

        public int GetGroupChatPort()
        {
            int port;
            if (groupChatPorts.TryDequeue(out port))
            {
                return port;
            }
            else
            {
                return -1;
            }
        }

        public void ReturnPrivateChatPort(int port)
        {
            lock(llock)
            {
                privateChatPorts.Enqueue(port);
            }
        }

        public void ReturnGroupChatPort(int port)
        {
            lock (llock)
            {
                groupChatPorts.Enqueue(port);
            }
        }

        /// <summary>
        /// Setting the value of a port which cannot be a portrange. On conflict it exits.
        /// </summary>
        /// <param name="portvalue">The stringvalue of the port.</param>
        /// <param name="port">The port which we want to set.</param>
        private bool InsertSinglePort(string portvalue, ref int port)
        {
            bool success = true;
            try
            {
                int portCandidate = int.Parse(portvalue);
                if(port >=-1)
                {
                    Console.WriteLine("PortManager error: port cannot be lower than -1! Exiting...");
                    success = false;
                }
                else
                {
                    if(takenPorts.Contains(portCandidate))
                    {
                        Console.WriteLine("PortManager error: conflict during ports on port " + portCandidate + "! Exiting...");
                        success = false;
                    }
                    else
                    {
                        takenPorts.Add(portCandidate);
                        port = portCandidate;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("PortManager error: Cannot parse given portvalue! Error message: " + e.Message + "Exiting...");
                success = false;
            }
            return success;
        }

        private bool InsertPortRange(string lower, string upper, ConcurrentQueue<int> portqueue)
        {
            bool success = true;
            try
            {
                int lowerbound = int.Parse(lower);
                int upperbound = int.Parse(upper);

                if(lowerbound>upperbound)
                {
                    Console.WriteLine("PortManager error: Lowerbound is higher than upperboiund! Exiting...");
                    success = false;
                    //Environment.Exit(0);
                }

                if(upperbound - lowerbound < 100)
                {
                    Console.WriteLine("PortManager warning: assigned less than 100 ports for a chattype!");
                }
                for(int portCandidate = lowerbound; portCandidate < upperbound+1 && success; portCandidate++)
                {
                    if(takenPorts.Contains(portCandidate))
                    {
                        success = false;
                        Console.WriteLine("PortManager error: conflict during ports on port " + portCandidate + "! Exiting...");
                    }
                    else
                    {
                        takenPorts.Add(portCandidate);
                        portqueue.Enqueue(portCandidate);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("PortManager error: error during parsing portrange boudns. Error message: " + e.Message + "Exiting...");
                success = false;
                //Environment.Exit(0);
            }
            return success;
        }

        public int Matchport { get { return matchport; } }
        public int Loginport { get { return loginport; } }
        public int Infoport { get { return infoport; } }
        public int Miscport { get { return miscport; } }
    }
}
