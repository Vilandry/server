using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace server.Model
{
    public class PortManager
    {
        private static PortManager manager;
        private static readonly object llock = new object();
        ConcurrentQueue<int> privateChatPorts;
        ConcurrentQueue<int> groupChatPorts;
        private int matchport;
        private int loginport;
        private int infoport;

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

            ///fill the privateChatPorts. Should get those numbers from a config file
            for(int i=11001; i<19999; i++)
            {
                privateChatPorts.Enqueue(i);
            }

            for(int i=21001; i<29999; i++)
            {
                groupChatPorts.Enqueue(i);
            }

            matchport = 9900;
            loginport = 11000;

            infoport = 9000; ///this should be const
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

        public int Matchport { get { return matchport; } }
        public int Loginport { get { return loginport; } }
        public int Infoport { get { return infoport; } }
    }
}
