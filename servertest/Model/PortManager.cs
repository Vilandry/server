using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;

namespace servertest.Model
{
    public class PortManager
    {
        private static PortManager manager;
        private static readonly object llock = new object();
        ConcurrentQueue<int> privateChatPorts;
        ConcurrentQueue<int> groupChatPorts;
        HashSet<int> takenPorts;
        private int matchport = 9900;
        private int loginport = 11000;
        private const int infoport = 9000;
        private int miscport = 9899;


        public static PortManager instance()
        {
            if (manager == null)
            {
                manager = new PortManager();

            }

            return manager;
        }

        private PortManager()
        {
            privateChatPorts = new ConcurrentQueue<int>();
            groupChatPorts = new ConcurrentQueue<int>();

            
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
            lock (llock)
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
                if (port >= -1)
                {
                    Console.WriteLine("PortManager error: port cannot be lower than -1! Exiting...");
                    success = false;
                }
                else
                {
                    if (takenPorts.Contains(portCandidate))
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
            catch (Exception e)
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

                if (lowerbound > upperbound)
                {
                    Console.WriteLine("PortManager error: Lowerbound is higher than upperboiund! Exiting...");
                    success = false;
                    //Environment.Exit(0);
                }

                if (upperbound - lowerbound < 100)
                {
                    Console.WriteLine("PortManager warning: assigned less than 100 ports for a chattype!");
                }
                for (int portCandidate = lowerbound; portCandidate < upperbound + 1 && success; portCandidate++)
                {
                    if (takenPorts.Contains(portCandidate))
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
            catch (Exception e)
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
