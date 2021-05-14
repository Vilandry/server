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
        public void AddFriendTest()
        {
            MiscController mc = MiscController.instance();
            Thread thread = new Thread(mc.handleRequests);
            thread.Start();

            string msg = "KNOCKNOCK|FRIEND|friend|candidate";

            TcpClient client = new TcpClient("localhost", PortManager.instance().Miscport);

            NetworkStream stream = client.GetStream();
            byte[] attempt = Encoding.Unicode.GetBytes(msg);

            stream.Write(attempt);
            Thread.Sleep(10);

            string res = Utility.ClientReadFromNetworkStream(stream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.

            Console.WriteLine(res + "|");

            Assert.AreEqual(res, "OK");
        }

        [TestMethod]
        public void BlockTest()
        {
            MiscController mc = MiscController.instance();
            Thread thread = new Thread(mc.handleRequests);
            thread.Start();

            string msg = "KNOCKNOCK|BLOCK|friend|candidate";

            TcpClient client = new TcpClient("localhost", PortManager.instance().Miscport);

            NetworkStream stream = client.GetStream();
            byte[] attempt = Encoding.Unicode.GetBytes(msg);

            stream.Write(attempt);
            Thread.Sleep(10);

            string res = Utility.ClientReadFromNetworkStream(stream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.

            Console.WriteLine(res + "|");

            Assert.AreEqual(res, "OK");
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

        #region MatchController

        [TestMethod]
        public void MatchControllerTest1()
        {
            MatchController mc = MatchController.instance();
            Thread thread = new Thread(mc.handleRequests);
            thread.Start();

            ///A|S|LS
            string Leftmsg = "KNOCKNOCK|candidate1|1|1|3";
            

            TcpClient Leftclient = new TcpClient("localhost", PortManager.instance().Matchport);
           


           
            NetworkStream leftstream = Leftclient.GetStream();
            byte[] leftattempt = Encoding.Unicode.GetBytes(Leftmsg);

            leftstream.Write(leftattempt);
            Thread.Sleep(10);

            string leftres = Utility.ClientReadFromNetworkStream(leftstream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.
            

            Console.WriteLine(leftres + "|");

            Assert.AreEqual(leftres, "OK");

            string rightmsg = "KNOCKNOCK|friend1|1|1|3";
            TcpClient Righttclient = new TcpClient("localhost", PortManager.instance().Matchport);
            NetworkStream rightstream = Righttclient.GetStream();

            byte[] rightattempt = Encoding.Unicode.GetBytes(rightmsg);

            rightstream.Write(rightattempt);

            string rightres = Utility.ClientReadFromNetworkStream(rightstream);
            Assert.AreEqual(rightres, "OK");
        }


        [TestMethod]
        public void MatchControllerTest2()
        {
            MatchController mc = MatchController.instance();
            Thread thread = new Thread(mc.handleRequests);
            thread.Start();

            ///A|S|LS
            string Leftmsg = "KNOCKNOCK|candidate1|1|1|1";


            TcpClient Leftclient = new TcpClient("localhost", PortManager.instance().Matchport);




            NetworkStream leftstream = Leftclient.GetStream();
            byte[] leftattempt = Encoding.Unicode.GetBytes(Leftmsg);

            leftstream.Write(leftattempt);
            Thread.Sleep(10);

            string leftres = Utility.ClientReadFromNetworkStream(leftstream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.


            Console.WriteLine(leftres + "|");


            string rightmsg = "KNOCKNOCK|friend1|1|1|1";
            TcpClient Righttclient = new TcpClient("localhost", PortManager.instance().Matchport);
            NetworkStream rightstream = Righttclient.GetStream();

            byte[] rightattempt = Encoding.Unicode.GetBytes(rightmsg);

            rightstream.Write(rightattempt);

            string rightres = Utility.ClientReadFromNetworkStream(rightstream);

            leftres = Utility.ClientReadFromNetworkStream(leftstream);
            rightres = Utility.ClientReadFromNetworkStream(rightstream);

            Assert.AreEqual("11001", leftres);
            Assert.AreEqual("11001", rightres);
        }

        [TestMethod]
        public void MatchControllerTest3()
        {
            MatchController mc = MatchController.instance();
            Thread thread = new Thread(mc.handleRequests);
            thread.Start();

            ///A|S|LS
            string Leftmsg = "KNOCKNOCK|candidate1|1|1|1";


            TcpClient Leftclient = new TcpClient("localhost", PortManager.instance().Matchport);




            NetworkStream leftstream = Leftclient.GetStream();
            byte[] leftattempt = Encoding.Unicode.GetBytes(Leftmsg);

            leftstream.Write(leftattempt);
            Thread.Sleep(10);

            string leftres = Utility.ClientReadFromNetworkStream(leftstream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.


            Console.WriteLine(leftres + "|");


            string rightmsg = "KNOCKNOCK|friend1|1|1|1";
            TcpClient Righttclient = new TcpClient("localhost", PortManager.instance().Matchport);
            NetworkStream rightstream = Righttclient.GetStream();

            byte[] rightattempt = Encoding.Unicode.GetBytes(rightmsg);

            rightstream.Write(rightattempt);

            string rightres = Utility.ClientReadFromNetworkStream(rightstream);

            leftres = Utility.ClientReadFromNetworkStream(leftstream);
            rightres = Utility.ClientReadFromNetworkStream(rightstream);




            ///A|S|LS
            string Leftmsg2 = "KNOCKNOCK|candidate2|1|1|1";


            TcpClient Leftclient2 = new TcpClient("localhost", PortManager.instance().Matchport);




            NetworkStream leftstream2 = Leftclient2.GetStream();
            byte[] leftattempt2 = Encoding.Unicode.GetBytes(Leftmsg2);

            leftstream2.Write(leftattempt2);
            Thread.Sleep(10);

            string leftres2 = Utility.ClientReadFromNetworkStream(leftstream2); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.


            Console.WriteLine(leftres2 + "|");


            string rightmsg2 = "KNOCKNOCK|friend2|1|1|1";
            TcpClient Righttclient2 = new TcpClient("localhost", PortManager.instance().Matchport);
            NetworkStream rightstream2 = Righttclient2.GetStream();

            byte[] rightattempt2 = Encoding.Unicode.GetBytes(rightmsg2);

            rightstream2.Write(rightattempt2);

            string rightres2 = Utility.ClientReadFromNetworkStream(rightstream2);

            leftres2 = Utility.ClientReadFromNetworkStream(leftstream2);
            rightres2 = Utility.ClientReadFromNetworkStream(rightstream2);

            Assert.AreEqual("11002", leftres2);
            Assert.AreEqual("11002", rightres2);
        }

        [TestMethod]
        public void MatchControllerTest4()
        {
            MatchController mc = MatchController.instance();
            Thread thread = new Thread(mc.handleRequests);
            thread.Start();

            ///A|S|LS
            string Leftmsg = "KNOCKNOCK|candidate|1|1|1";


            TcpClient Leftclient = new TcpClient("localhost", PortManager.instance().Matchport);




            NetworkStream leftstream = Leftclient.GetStream();
            byte[] leftattempt = Encoding.Unicode.GetBytes(Leftmsg);

            leftstream.Write(leftattempt);
            Thread.Sleep(10);

            string leftres = Utility.ClientReadFromNetworkStream(leftstream); ///note that its the server's read function, which is different and requires the KNOCKNOCK| trailer.


            Console.WriteLine(leftres + "|");


            string rightmsg = "KNOCKNOCK|friend|1|1|1";
            TcpClient Righttclient = new TcpClient("localhost", PortManager.instance().Matchport);
            NetworkStream rightstream = Righttclient.GetStream();

            byte[] rightattempt = Encoding.Unicode.GetBytes(rightmsg);

            rightstream.Write(rightattempt);

            string rightres = Utility.ClientReadFromNetworkStream(rightstream);




            ///A|S|LS
            string Leftmsg2 = "KNOCKNOCK|candidate2|1|1|1";


            TcpClient Leftclient2 = new TcpClient("localhost", PortManager.instance().Matchport);




            NetworkStream leftstream2 = Leftclient2.GetStream();
            byte[] leftattempt2 = Encoding.Unicode.GetBytes(Leftmsg2);

            leftstream2.Write(leftattempt2);
            Thread.Sleep(10);
            leftres = Utility.ClientReadFromNetworkStream(leftstream);
            string leftres2 = Utility.ClientReadFromNetworkStream(leftstream2);

            Thread.Sleep(100);
        }
        #endregion
    }
}
