using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System;
using servertest.Controller;
using servertest.Model;

namespace servertest
{
    [TestClass]
    public class UnitTest
    {
        #region LoginController
        
        [TestMethod]
        public void RegisterTest_OK()
        {
            LoginController lc = LoginController.instance();
            Thread thread = new Thread(lc.logincontrol);
            thread.Start();

            string username = "NotTaken";
            int age = 1;
            int gender = 1;
            string password = "nottaken"; ///lets assume its the hash of the pwd

            string msg = "KNOCKNOCK|REGISTER|" + username + "|" + password + "|" + age + "|" + gender;

            TcpClient client = new TcpClient("localhost", PortManager.instance().Loginport);

            NetworkStream stream = client.GetStream();
            byte[] attempt = Encoding.Unicode.GetBytes(msg);

            stream.Write(attempt);
            Thread.Sleep(10);

            string res = Utility.ClientReadFromNetworkStream(stream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.

            Console.WriteLine(res + "|");

            Assert.AreEqual(res, "OK|");
        }

        [TestMethod]
        public void RegisterTest_NOTOK()
        {
            LoginController lc = LoginController.instance();
            Thread thread = new Thread(lc.logincontrol);
            thread.Start();

            string username = "Taken";
            int age = 0;
            int gender = 1;
            string password = "nottaken"; ///lets assume its the hash of the pwd

            string msg = "KNOCKNOCK|REGISTER|" + username + "|" + password + "|" + age + "|" + gender;

            TcpClient client = new TcpClient("localhost", PortManager.instance().Loginport);

            NetworkStream stream = client.GetStream();
            byte[] attempt = Encoding.Unicode.GetBytes(msg);

            stream.Write(attempt);
            Thread.Sleep(10);

            string res = Utility.ClientReadFromNetworkStream(stream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.

            Console.WriteLine(res + "|");

            Assert.AreEqual(res, "ER|-1|-1");
        }


        [TestMethod]
        public void LOGINTest_OK()
        {
            LoginController lc = LoginController.instance();
            Thread thread = new Thread(lc.logincontrol);
            thread.Start();

            string username = "NotTaken";
            string password = "nottaken"; ///lets assume its the hash of the pwd

            string msg = "KNOCKNOCK|LOGIN|" + username + "|" + password;

            TcpClient client = new TcpClient("localhost", PortManager.instance().Loginport);

            NetworkStream stream = client.GetStream();
            byte[] attempt = Encoding.Unicode.GetBytes(msg);

            stream.Write(attempt);
            Thread.Sleep(10);

            string res = Utility.ClientReadFromNetworkStream(stream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.

            Console.WriteLine(res + "|");

            Assert.AreEqual(res, "OK|0|1");
        }

        [TestMethod]
        public void LOGINTest_NOTOK()
        {
            LoginController lc = LoginController.instance();
            Thread thread = new Thread(lc.logincontrol);
            thread.Start();

            string username = "NotTaken";
            string password = "nottakendfghfds"; ///lets assume its the hash of the pwd

            string msg = "KNOCKNOCK|LOGIN|" + username + "|" + password;

            TcpClient client = new TcpClient("localhost", PortManager.instance().Loginport);

            NetworkStream stream = client.GetStream();
            byte[] attempt = Encoding.Unicode.GetBytes(msg);

            stream.Write(attempt);
            Thread.Sleep(10);

            string res = Utility.ClientReadFromNetworkStream(stream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.

            Console.WriteLine(res + "|");

            Assert.AreEqual(res, "ER|-1|-1");
        }
        #endregion

        #region MiscController
        [TestMethod]
        public void FriendlisttTest()
        {
            MiscController mc = MiscController.instance();
            Thread thread = new Thread(mc.handleRequests);
            thread.Start();

            string msg = "KNOCKNOCK|FRIENDLOAD|friend";

            TcpClient client = new TcpClient("localhost", PortManager.instance().Miscport);

            NetworkStream stream = client.GetStream();
            byte[] attempt = Encoding.Unicode.GetBytes(msg);

            stream.Write(attempt);
            Thread.Sleep(10);

            string res = Utility.ClientReadFromNetworkStream(stream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.

            Console.WriteLine(res + "|");

            string[] resparts = res.Split("!");

            string[] mutual = resparts[0].Split("|");
            string[] onlysender = resparts[1].Split("|");
            string[] onlytarget = resparts[2].Split("|");

            Assert.AreEqual(mutual.Length, 1);
            Assert.AreEqual(onlysender.Length, 1);
            Assert.AreEqual(onlytarget.Length, 1);

            Assert.AreEqual(mutual[0],"mutual");
            Assert.AreEqual(onlysender[0], "OnlySender");
            Assert.AreEqual(onlytarget[0], "OnlyTarget");
        }

        [TestMethod]
        public void ConvLoadTest()
        {
            MiscController mc = MiscController.instance();
            Thread thread = new Thread(mc.handleRequests);
            thread.Start();

            string msg = "KNOCKNOCK|CONVLOAD|0|friend|NotTaken";

            TcpClient client = new TcpClient("localhost", PortManager.instance().Miscport);

            NetworkStream stream = client.GetStream();
            byte[] attempt = Encoding.Unicode.GetBytes(msg);

            stream.Write(attempt);
            Thread.Sleep(10);

            string res = Utility.ClientReadFromNetworkStream(stream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.

            Console.WriteLine(res + "|");


            Assert.AreEqual(res, "ok");

        }

        [TestMethod]
        public void ConvSaveTest()
        {
            MiscController mc = MiscController.instance();
            Thread thread = new Thread(mc.handleRequests);
            thread.Start();

            string msg = "KNOCKNOCK|CONVSAVE|0|friend|NotTaken|friend";

            TcpClient client = new TcpClient("localhost", PortManager.instance().Miscport);

            NetworkStream stream = client.GetStream();
            byte[] attempt = Encoding.Unicode.GetBytes(msg);

            stream.Write(attempt);
            Thread.Sleep(10);

            string res = Utility.ClientReadFromNetworkStream(stream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.

            Console.WriteLine(res + "|");


            Assert.AreEqual(res, "INSERT");

            attempt = Encoding.Unicode.GetBytes("KNOCKNOCK|0|friend|NotTaken|friend");

            stream.Write(attempt);

            Thread.Sleep(10);
            res = Utility.ClientReadFromNetworkStream(stream);

            Assert.AreEqual(res, "OK");
        }

        [TestMethod]
        public void ConvSaveTest2()
        {
            MiscController mc = MiscController.instance();
            Thread thread = new Thread(mc.handleRequests);
            thread.Start();

            string msg = "KNOCKNOCK|CONVSAVE|1|friend|NotTaken|NotTaken";

            TcpClient client = new TcpClient("localhost", PortManager.instance().Miscport);

            NetworkStream stream = client.GetStream();
            byte[] attempt = Encoding.Unicode.GetBytes(msg);

            stream.Write(attempt);
            Thread.Sleep(10);

            string res = Utility.ClientReadFromNetworkStream(stream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.

            Console.WriteLine(res + "|");


            Assert.AreEqual(res, "OK");

        }
        #endregion
    }
}
