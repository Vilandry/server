using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System;
using System.Diagnostics;

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

            Assert.AreEqual("OK|", res);
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

            Assert.AreEqual("ER|-1|-1", res);
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

            Assert.AreEqual("OK|0|1", res);
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

            Assert.AreEqual("ER|-1|-1", res);
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
        public void MatchControllerTest1_CONNECT()
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

            try { mc.Server.Stop(); thread.Abort(); } catch (Exception e) { }
        }


        [TestMethod]
        public void MatchControllerTest2_MATCH()
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
            try { mc.Server.Stop(); thread.Abort(); } catch (Exception e) { }
        }

        [TestMethod]
        public void MatchControllerTest3_MULTIPLE()
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
            try { mc.Server.Stop(); thread.Abort(); } catch (Exception e) { }
        }

        [TestMethod]
        public void MatchControllerTest4_BLOCK()
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

            ///PORT
            leftres = Utility.ClientReadFromNetworkStream(leftstream);
            string leftres2 = Utility.ClientReadFromNetworkStream(leftstream2);

            ///ID
            Thread.Sleep(100);
            Trace.WriteLine(leftres);
            leftres = Utility.ClientReadFromNetworkStream(leftstream);
            Trace.WriteLine(leftres);


            string[] idArgs = leftres.Split("|");

            Assert.AreEqual("candidate", Array.Find(idArgs, candidate => (candidate == "candidate")) );
            Assert.AreEqual("candidate2", Array.Find(idArgs, candidate => (candidate == "candidate2")));

            try { mc.Server.Stop(); thread.Abort(); } catch (Exception e) { }
        }


        [TestMethod]
        public void MatchControllerTest5_LEAVE()
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

            Leftmsg = "KNOCKNOCK|!LEAVE|candidate1";
            leftattempt = Encoding.Unicode.GetBytes(Leftmsg);

            leftstream.Write(leftattempt);


            string rightmsg = "KNOCKNOCK|friend1|1|1|1";
            TcpClient Righttclient = new TcpClient("localhost", PortManager.instance().Matchport);
            NetworkStream rightstream = Righttclient.GetStream();

            byte[] rightattempt = Encoding.Unicode.GetBytes(rightmsg);

            rightstream.Write(rightattempt);

            string rightres = Utility.ClientReadFromNetworkStream(rightstream);
            Trace.WriteLine("righres: " + rightres);



            ///A|S|LS
            string Leftmsg2 = "KNOCKNOCK|candidate2|1|1|1";


            TcpClient Leftclient2 = new TcpClient("localhost", PortManager.instance().Matchport);




            NetworkStream leftstream2 = Leftclient2.GetStream();
            byte[] leftattempt2 = Encoding.Unicode.GetBytes(Leftmsg2);

            
            leftstream2.Write(leftattempt2);

            ///PORT
            rightres = Utility.ClientReadFromNetworkStream(rightstream);
            Thread.Sleep(10);

            ///ID
            rightres = Utility.ClientReadFromNetworkStream(rightstream);


            string[] idArgs = rightres.Split("|");

            Assert.AreEqual("friend1", Array.Find(idArgs, candidate => (candidate == "friend1")));
            Assert.AreEqual("candidate2", Array.Find(idArgs, candidate => (candidate == "candidate2")));


            try { mc.Server.Stop(); thread.Abort(); } catch (Exception e) { }
        }

        [TestMethod]
        public void MatchControllerTest6_INQUEUE()
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

            string rightmsg = "KNOCKNOCK|candidate1|1|1|2";
            TcpClient Righttclient = new TcpClient("localhost", PortManager.instance().Matchport);
            NetworkStream rightstream = Righttclient.GetStream();

            byte[] rightattempt = Encoding.Unicode.GetBytes(rightmsg);

            rightstream.Write(rightattempt);

            string rightres = Utility.ClientReadFromNetworkStream(rightstream);

            Thread.Sleep(10);

            //Assert.AreEqual(rightres, "OK");

            Assert.AreEqual("ER|INQUEUE", rightres);


            try { mc.Server.Stop(); thread.Abort(); } catch (Exception e) { }
        }
        #endregion


        #region PrivateChatController

        [TestMethod]
        public void PrivateChattingTest()
        {
            PrivateChatController pc = new PrivateChatController(11111, CHATTPYE.PRIVATE);


            Thread thread = new Thread(pc.handleConnecting);
            thread.Start();


            TcpClient leftClient = new TcpClient("localhost", 11111);
            NetworkStream leftstream = leftClient.GetStream();

            TcpClient rightClient = new TcpClient("localhost", 11111);
            NetworkStream rightstream = rightClient.GetStream();

            string validate = "KNOCKNOCK|";

            string msg = validate + "leftUser|Hi rightuser!";

            byte[] attempt = Encoding.Unicode.GetBytes(msg);

            leftstream.Write(attempt);

            string leftRecieved = Utility.ClientReadFromNetworkStream(leftstream);
            string rightRecieved = Utility.ClientReadFromNetworkStream(rightstream);

            Assert.AreEqual("leftUser|Hi rightuser!", leftRecieved);
            Assert.AreEqual("leftUser|Hi rightuser!", rightRecieved);

            msg = validate + "leftUser|Are You here?";
            attempt = Encoding.Unicode.GetBytes(msg);

            leftstream.Write(attempt);

            leftRecieved = Utility.ClientReadFromNetworkStream(leftstream);
            rightRecieved = Utility.ClientReadFromNetworkStream(rightstream);

            Assert.AreEqual("leftUser|Are You here?", leftRecieved);
            Assert.AreEqual("leftUser|Are You here?", rightRecieved);


            msg = validate + "rightUser|Sorry :( Yeah, I'm here :) 😊";
            attempt = Encoding.Unicode.GetBytes(msg);

            rightstream.Write(attempt);

            leftRecieved = Utility.ClientReadFromNetworkStream(leftstream);
            rightRecieved = Utility.ClientReadFromNetworkStream(rightstream);

            Assert.AreEqual("rightUser|Sorry :( Yeah, I'm here :) 😊", leftRecieved);
            Assert.AreEqual("rightUser|Sorry :( Yeah, I'm here :) 😊", rightRecieved);
        }


        #endregion
    }
}
